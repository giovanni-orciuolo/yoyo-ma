using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using DSharpPlus.VoiceNext;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using yoyo_bot.src.bot.entities;

namespace yoyo_bot.src.bot.services
{
    /// <summary>
    /// Manages music reproduction using ffmpeg and VoiceNext.
    /// You can also use it to connect to voice channels
    /// </summary>
    class MusicService
    {
        /// <summary>
        /// Map where guild id -> active guild music channel
        /// </summary>
        public ConcurrentDictionary<ulong, GuildMusicChannel> MusicChannels { get; } = new ConcurrentDictionary<ulong, GuildMusicChannel>();

        /// <summary>
        /// Joins a voice channel
        /// </summary>
        /// <param name="vnext">VoiceNext instance</param>
        /// <param name="voiceChannel">Voice channel to join</param>
        /// <returns></returns>
        public async Task<GuildMusicChannel> JoinVoiceChannel(VoiceNextExtension vnext, DiscordChannel voiceChannel)
        {
            var vnc = vnext.GetConnection(voiceChannel.Guild);
            if (vnc != null)
                return;

            var channel = new GuildMusicChannel(voiceChannel.Guild);
            if (!this.MusicChannels.TryAdd(voiceChannel.GuildId, channel))
            {
                // If the add fails, it means there is already a channel for this guild, recover it
                this.MusicChannels.TryGetValue(voiceChannel.GuildId, out channel);
            }
            channel.IsConnected = true;

            vnc = await vnext.ConnectAsync(voiceChannel);
            return channel;
        }

        /// <summary>
        /// Leaves the voice channel where the bot is currently connected for a given guild
        /// </summary>
        /// <param name="vnext">VoiceNext instance</param>
        /// <param name="guild">Guild reference</param>
        public async Task LeaveVoiceChannel(VoiceNextExtension vnext, DiscordGuild guild)
        {
            var vnc = vnext.GetConnection(guild);
            if (vnc == null)
                throw new InvalidOperationException($"I'm not connected to any voice channel! {DiscordEmoji.FromName(vnext.Client, ":thinking:")}");

            if (!this.MusicChannels.TryGetValue(guild.Id, out GuildMusicChannel channel))
                throw new InvalidOperationException("No music channel associated with this guild!");

            if (await this.TryDequeueSong(channel) && !channel.Queue.IsEmpty)
                channel.Queue.Clear();

            channel.IsConnected = false;
            vnc.Dispose();
        }

        /// <summary>
        /// Plays music through the music process
        /// </summary>
        /// <param name="vnext">VoiceNext instance</param>
        /// <param name="guild">Guild reference</param>
        /// <param name="song_path">Path to MP3 music file</param>
        /// <returns></returns>
        public async Task PlayFromMemory(VoiceNextExtension vnext, DiscordGuild guild, DiscordMember requested_by, string song_path)
        {
            var vnc = vnext.GetConnection(guild);
            if (vnc == null)
                throw new InvalidOperationException($"I'm not connected to any voice channel! {DiscordEmoji.FromName(vnext.Client, ":thinking:")}");

            if (!File.Exists(song_path))
                throw new FileNotFoundException($"Music file not found! {DiscordEmoji.FromName(vnext.Client, ":(")}");

            if (!this.MusicChannels.TryGetValue(guild.Id, out GuildMusicChannel channel))
                throw new InvalidOperationException("No music channel associated with this guild!");

            this.EnqueueSong(channel, new MusicData
            {
                Source = song_path,
                Channel = channel,
                RequestedBy = requested_by,
                MusicType = MusicTypes.MEMORY
            });

            // Something it's being already played, limit to just enqueueing
            if (channel.IsPlaying)
                return;

            var txStream = vnc.GetTransmitStream();
            txStream.VolumeModifier = channel.Volume / 100f;

            // Start speaking
            channel.IsPlaying = true;
            channel.MusicProc = new MusicProcess(song_path, ProcessStartMode.OUTPUT);

            var ffout = channel.MusicProc.FFMpeg.StandardOutput.BaseStream;
            await ffout.CopyToAsync(txStream);
            await txStream.FlushAsync().ConfigureAwait(false);

            await vnc.WaitForPlaybackFinishAsync();

            // Stop speaking (also sets IsPlaying to false)
            await this.TryDequeueSong(channel);
        }

        /// <summary>
        /// Stops any currently playing music but does not disconnect from the voice channel
        /// </summary>
        /// <param name="vnext">VoiceNext instance</param>
        /// <param name="guild">Guild reference</param>
        /// <returns></returns>
        public async Task Stop(VoiceNextExtension vnext, DiscordGuild guild)
        {
            var vnc = vnext.GetConnection(guild);
            if (vnc == null)
                throw new InvalidOperationException($"I'm not connected to any voice channel! {DiscordEmoji.FromName(vnext.Client, ":thinking:")}");

            if (!this.MusicChannels.TryGetValue(guild.Id, out GuildMusicChannel channel))
                throw new InvalidOperationException("No music channel associated with this guild!");

            await this.TryDequeueSong(channel);
        }

        /// <summary>
        /// Sets the volume for the channel associated with a guild
        /// </summary>
        /// <param name="guild">Guild reference</param>
        /// <param name="volume">Volume to set [0, 100]</param>
        public void SetVolume(DiscordGuild guild, ushort volume)
        {
            if (this.MusicChannels.TryGetValue(guild.Id, out GuildMusicChannel channel))
                channel.Volume = volume;
            else
            {
                var newMusicChannel = new GuildMusicChannel(guild) {
                    Volume = volume
                };
                this.MusicChannels.TryAdd(guild.Id, newMusicChannel);
            }
        }

        /// <summary>
        /// Permanently removes music channel from guild
        /// Best to call it when the bot leaves a guild
        /// </summary>
        /// <param name="guild">Guild reference</param>
        public void RemoveMusicChannel(DiscordGuild guild)
        {
            this.MusicChannels.TryRemove(guild.Id, out GuildMusicChannel removed);
        }

        /// <summary>
        /// Enqueues a new song in the relative channel queue
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public void EnqueueSong(GuildMusicChannel channel, MusicData item)
        {
            if (channel.Queue.Count >= GuildMusicChannel.QUEUE_CAPACITY)
                throw new IndexOutOfRangeException($"Queue reached maximum capacity of {GuildMusicChannel.QUEUE_CAPACITY}! Get a grip man...");

            channel.Queue.Enqueue(item);
        }

        /// <summary>
        /// Kills music process (if necessary) and dequeues current song from channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task<bool> TryDequeueSong(GuildMusicChannel channel)
        {
            if (channel.MusicProc != null)
            {
                await channel.MusicProc.Kill(ProcessStartMode.OUTPUT);
                channel.MusicProc = null;
            }
            channel.IsPlaying = false;
            return channel.Queue.TryDequeue(out MusicData result);
        }

        /// <summary>
        /// Creates embedded message to visualize queue for a guild
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public DiscordEmbed CreateQueueEmbedForGuild(CommandContext ctx)
        {
            if (!this.MusicChannels.TryGetValue(ctx.Guild.Id, out GuildMusicChannel channel))
                throw new InvalidOperationException("No music channel is yet associated for this guild...");

            if (channel.Queue.Count == 0)
                throw new InvalidOperationException("The queue for this guild is empty at the moment. Try to play something using 'yo play'!");

            var queueArray = channel.Queue.ToArray();
            var currentSong = queueArray[0];

            var builder = new DiscordEmbedBuilder
            {
                Title = $"Queue for {ctx.Guild.Name}",
                Description = 
                    $"\n\n__Now Playing:__\n" +
                    $"```{currentSong.GetFormattedSource()} | Requested by {currentSong.RequestedBy.Username}```\n\n" +
                    $"{DiscordEmoji.FromName(ctx.Client, ":arrow_forward:")} UP NEXT:\n\n" +
                    $"{(queueArray.Length == 1 ? $"**Nothing...** {DiscordEmoji.FromName(ctx.Client, ":(")}" : "")}",
                Color = DiscordColor.SpringGreen,
            }.WithFooter(iconUrl: ctx.Member.GetAvatarUrl(ImageFormat.Png));

            if (queueArray.Length > 1)
            {
                for (int i = 1; i < queueArray.Length; ++i)
                {
                    var queueSong = queueArray[i];
                    builder.AddField($"{i}. {queueSong.GetFormattedSource()} | Requested by {queueSong.RequestedBy.Username}", "");
                }
            }

            return builder.Build();
        }
    }
}
