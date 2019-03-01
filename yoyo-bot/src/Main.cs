using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using yoyo_bot.src.bot;

namespace yoyo_bot
{
    class BotMain
    {
        static YoYoBot bot;

        static void Main(string[] args)
        {
            Console.Title = "Yo-Yo Ma";
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            bot = new YoYoBot(GetBotToken());

            await bot.ConnectAsync();
            await Task.Delay(-1); // Makes this code run forever
        }

        static string GetBotToken()
        {
            // Grab the token from environment or from local file
            string token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
            if (token == null)
            {
                Console.WriteLine("Discord Token not found in environment, I'm going to use local text file to read it.");
                FileStream fileStream = new FileStream(@"../../../token.txt", FileMode.Open, FileAccess.Read);
                using (StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }
            }
            else return token;
        }
    }
}
