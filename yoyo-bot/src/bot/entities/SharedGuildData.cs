using DSharpPlus.Entities;

namespace yoyo_bot.src.bot.entities
{
    /// <summary>
    /// Base container for data shared across many guilds
    /// </summary>
    class SharedGuildData
    {
        public SharedGuildData(DiscordGuild guild)
        {
            this.Guild = guild;
        }

        /// <summary>
        /// Discord guild associated with this data
        /// </summary>
        public DiscordGuild Guild { get; }

    }
}
