using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using yoyo_bot.src.bot.commands;
using yoyo_bot.src.bot.services;

namespace yoyo_bot.src.bot
{
    /// <summary>
    /// Main class for YoYo-Ma bot.
    /// Instantiate it with a token to setup the whole system
    /// </summary>
    class YoYoBot : DiscordClient
    {
        public static string[] COMMAND_PREFIXES = { "yo " };

        private CommandsNextExtension CommandsNext;
        private readonly VoiceNextExtension VoiceNext;

        public YoYoBot(string token) : base(new YoYoConfig(token).Config)
        {
            CommandsNext = this.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = COMMAND_PREFIXES,
                EnableDms = false,
                CaseSensitive = false,
                IgnoreExtraArguments = false,
                EnableMentionPrefix = true,

                Services = new ServiceCollection()
                    .AddSingleton(new MusicService())
                    .BuildServiceProvider()
            });

            CommandsNext.RegisterCommands<ModCommands>();
            CommandsNext.RegisterCommands<MusicCommands>();
            CommandsNext.RegisterCommands<MemeCommands>();

            VoiceNext = this.UseVoiceNext();

            SetupEvents();
        }

        public async Task<DiscordMember> FindMemberByName(DiscordGuild guild, string username)
        {
            foreach (var member in await guild.GetAllMembersAsync())
                if (member.Username.ToLower().Contains(username.ToLower()))
                    return member;
            return null;
        }

        private void SetupEvents()
        {
            GuildCreated += async e =>
            {
                await SendMessageAsync(e.Guild.GetDefaultChannel(),
                    "YoYo-Ma joined this server! " +
                    "May I serve you?"
                );
            };

            GuildDeleted += async e =>
            {
                await SendMessageAsync(e.Guild.GetDefaultChannel(), "Bye bye!");
            };

            // PLACEHOLDER GHETTO CODE, needs to be replaced with a proper API endpoint to listen for
            MessageCreated += async e =>
            {
                // Need to add a proper check, atm any bot is valid
                if (e.Author.IsBot)
                    return;

                string message = e.Message.Content;
                if (!message.StartsWith(COMMAND_PREFIXES[0]))
                    return;

                string message_strip = message.Substring(3);
                string[] commands = message_strip.Split(" ");
                if (commands.Length < 2)
                    return;

                string command = commands[0];
                string user = commands[1];

                if (command.Contains("mute-wh") || command.Contains("unmute-wh"))
                {
                    await (await FindMemberByName(e.Guild, user)).SetMuteAsync(command == "mute-wh");
                }
            };
        }
    }
}
