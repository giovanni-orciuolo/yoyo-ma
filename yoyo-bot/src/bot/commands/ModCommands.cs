using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus;
using System.Threading.Tasks;

namespace yoyo_bot.src.bot
{
    /// <summary>
    /// Declares commands to help with server moderation
    /// </summary>
    class ModCommands : BaseCommandModule
    {
        [Command("mute"), RequirePermissions(Permissions.MuteMembers)]
        [Description("Mutes a mentioned person, requires mute permission")]
        public async Task Mute(CommandContext ctx, DiscordMember toMute)
        {
            await toMute.SetMuteAsync(true);
            await ctx.RespondAsync($":mute: {toMute.DisplayName} just got muted! He-he");
        }

        [Command("unmute"), RequirePermissions(Permissions.MuteMembers)]
        [Description("Unmutes a mentioned person, requires mute permission")]
        public async Task Unmute(CommandContext ctx, DiscordMember toMute)
        {
            await toMute.SetMuteAsync(false);
            await ctx.RespondAsync($":wave: {toMute.DisplayName} got unmuted!");
        }
    }
}
