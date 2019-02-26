using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System;
using System.Threading.Tasks;

namespace yoyo_bot.src.bot.music
{
    /// <summary>
    /// Declares commands to interact with music
    /// </summary>
    class MusicCommands : BaseCommandModule
    {
        public MusicService Music { get; }

        public MusicCommands(MusicService music)
        {
            this.Music = music;
        }

        [Command("play"), Aliases("sing")]
        [Description("Joins your voice channel and plays music")]
        [RequireUserPermissions(Permissions.ManageChannels)]
        public async Task Play(CommandContext ctx, [RemainingText] string song)
        {
            try
            {
                var voiceNext = ctx.Client.GetVoiceNext();

                var userVoiceChannel = ctx.Member.VoiceState?.Channel;
                var botVoiceChannel = ctx.Guild.CurrentMember.VoiceState?.Channel;

                // First of all join the voice channel
                if (userVoiceChannel == null)
                {
                    await ctx.RespondAsync($"{ctx.User.Username}, you are not connected to any voice channel... {DiscordEmoji.FromName(ctx.Client, ":thinking:")}");
                    return;
                }
                if (botVoiceChannel != null && userVoiceChannel != botVoiceChannel)
                {
                    await ctx.RespondAsync($"{ctx.User.Username}, you need to be in the same channel with me!");
                }

                await this.Music.JoinVoiceChannel(voiceNext, userVoiceChannel);

                // "song" can be 3 things
                // 1. It can be nothing.
                if (song == "")
                {
                    // In this case, I need to show the user a list of downloaded songs that the bot owns in db
                    // Not yet implemented
                }
                else if (song.Contains("youtube.com"))
                {
                    // In this case, I will call Lavalink service to play music directly from Youtube
                    // Not yet implemented
                }
                else
                {
                    // In this case, the user is passing a specific song name that the bot owns in db
                    // Search for it and, if not found, call Youtube service to search for it on youtube, as a last resort
                    await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":musical_note:")} Playing audio file from memory '{song}'");
                    await this.Music.Play(voiceNext, ctx.Guild, song);
                }
            }
            catch (Exception e)
            {
                await CommandError.Handle(ctx, e);
            }
        }

        [Command("stop"), Aliases("kill")]
        [Description("Stops currently playing music but does not leave the channel")]
        [RequireUserPermissions(Permissions.ManageChannels)]
        public async Task Stop(CommandContext ctx)
        {
            try
            {
                await this.Music.Stop(ctx.Client.GetVoiceNext(), ctx.Guild);
            }
            catch (Exception e)
            {
                await CommandError.Handle(ctx, e);
            }
        }

        [Command("leave"), Aliases(new string[] { "disconnect", "quit" })]
        [Description("Leaves your voice channel")]
        [RequireUserPermissions(Permissions.ManageChannels)]
        public async Task Leave(CommandContext ctx)
        {
            try
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":x:")} Ok ok I leave no problem...");
                await this.Music.LeaveVoiceChannel(ctx.Client.GetVoiceNext(), ctx.Guild);
            }
            catch (Exception e)
            {
                await CommandError.Handle(ctx, e);
            }
        }

        [Command("set-volume")]
        [Description("Modifies the volume of the bot, pass a value between 0 and 100")]
        [RequireUserPermissions(Permissions.ManageChannels)]
        public async Task SetVolume(CommandContext ctx, [RemainingText] ushort volume)
        {
            try
            {
                this.Music.SetVolume(ctx.Guild, volume);
                await ctx.RespondAsync(
                    $"{DiscordEmoji.FromName(ctx.Client, volume >= 50 ? ":loud_sound:" : ":sound:")} Set music volume to {volume}!"
                );
            }
            catch (Exception e)
            {
                await CommandError.Handle(ctx, e);
            }
        }

        [Command("play-test")]
        [RequireUserPermissions(Permissions.ManageChannels)]
        public async Task PlayTest(CommandContext ctx)
        {
            await this.Play(ctx, @"C:\Nanatsu no taizai Opening.mp3");
        }
    }
}
