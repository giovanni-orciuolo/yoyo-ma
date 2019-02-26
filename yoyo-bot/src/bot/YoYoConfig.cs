using DSharpPlus;

namespace yoyo_bot.src.bot
{
    class YoYoConfig
    {
        public DiscordConfiguration Config { get; }

        public YoYoConfig(string token)
        {
            Config = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,

#if DEBUG
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug,
#else
                UseInternalLogHandler = false,
                LogLevel = LogLevel.Info,
#endif

                AutoReconnect = true,
                ReconnectIndefinitely = true,
                GatewayCompressionLevel = GatewayCompressionLevel.Stream,
                LargeThreshold = 250,
            };
        }
    }
}
