using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace yoyo_bot.src.bot.commands
{
    /// <summary>
    /// Meme commands yeah xddd
    /// </summary>
    class MemeCommands : BaseCommandModule
    {
        private static readonly string ANGELO = @"../../../src/resources/angelo.png";

        [Command("angelo")]
        public async Task Angelo(CommandContext ctx)
        {
            await ctx.RespondWithFileAsync(ANGELO);
        }
    }
}
