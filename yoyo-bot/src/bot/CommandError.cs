using DSharpPlus.CommandsNext;
using System;
using System.Threading.Tasks;

namespace yoyo_bot.src.bot
{
    class CommandError
    {
        public static async Task Handle(CommandContext ctx, Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Yo-Yo Ma] ERROR during execution of '{ctx.Command.Name}' command: {e}");
            Console.ResetColor();
            await ctx.RespondAsync($"{e.Message}");
        }
    }
}
