using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace yoyo_bot.src.bot.music
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
        public async Task JoinVoiceChannel(VoiceNextExtension vnext, DiscordChannel voiceChannel)
        {
            var vnc = vnext.GetConnection(voiceChannel.Guild);
            if (vnc != null)
                throw new InvalidOperationException($"I'm already connected in channel '{vnc.Channel?.Name}' on this guild! :triumph:");

            var channel = new GuildMusicChannel(voiceChannel.Guild);
            if (!this.MusicChannels.TryAdd(voiceChannel.GuildId, channel))
            {
                // If the add fails, it means there is already a channel for this guild, recover it
                this.MusicChannels.TryGetValue(voiceChannel.GuildId, out channel);
            }
            channel.IsConnected = true;

            vnc = await vnext.ConnectAsync(voiceChannel);
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

            if (channel.MusicProc != null)
            {
                await channel.MusicProc.Kill(ProcessStartMode.OUTPUT);
                channel.MusicProc = null;
            }

            channel.IsConnected = false;
            channel.IsPlaying = false;
            vnc.Dispose();
        }

        /// <summary>
        /// Plays music through the music process
        /// </summary>
        /// <param name="vnext">VoiceNext instance</param>
        /// <param name="guild">Guild reference</param>
        /// <param name="song_path">Path to MP3 music file</param>
        /// <returns></returns>
        public async Task Play(VoiceNextExtension vnext, DiscordGuild guild, string song_path)
        {
            var vnc = vnext.GetConnection(guild);
            if (vnc == null)
                throw new InvalidOperationException($"I'm not connected to any voice channel! {DiscordEmoji.FromName(vnext.Client, ":thinking:")}");

            if (!File.Exists(song_path))
                throw new FileNotFoundException($"Music file not found! {DiscordEmoji.FromName(vnext.Client, ":(")}");

            // Get channel associated with this guild
            if (!this.MusicChannels.TryGetValue(guild.Id, out GuildMusicChannel channel))
                throw new InvalidOperationException("No music channel associated with this guild!");

            // Setup transmit stream
            var txStream = vnc.GetTransmitStream();
            txStream.VolumeModifier = channel.Volume / 100f;

            // Start speaking
            channel.MusicProc = new MusicProcess(song_path, ProcessStartMode.OUTPUT);
            channel.IsPlaying = true;

            var ffout = channel.MusicProc.FFMpeg.StandardOutput.BaseStream;
            await ffout.CopyToAsync(txStream);
            await txStream.FlushAsync().ConfigureAwait(false);

            await vnc.WaitForPlaybackFinishAsync();

            // Stop speaking
            channel.IsPlaying = false;
            await channel.MusicProc.Kill(ProcessStartMode.OUTPUT);
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

            // Get channel associated with this guild
            if (!this.MusicChannels.TryGetValue(guild.Id, out GuildMusicChannel channel))
                throw new InvalidOperationException("No music channel associated with this guild!");

            channel.IsPlaying = false;
            await channel.MusicProc.Kill(ProcessStartMode.OUTPUT);
            channel.MusicProc = null;
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

    }
}
