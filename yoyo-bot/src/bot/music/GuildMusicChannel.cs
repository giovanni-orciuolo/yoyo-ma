using DSharpPlus.Entities;
using yoyo_bot.src.bot.entities;

namespace yoyo_bot.src.bot.music
{
    /// <summary>
    /// Represents a guild specific music channel
    /// </summary>
    class GuildMusicChannel : SharedGuildData
    {
        public GuildMusicChannel(DiscordGuild guild)
            : base(guild)
        {
        }

        /// <summary>
        /// Gets whether the bot is connected to a voice channel
        /// </summary>
        public bool IsConnected { get; set; } = false;

        /// <summary>
        /// Gets whether a track is currently playing
        /// </summary>
        public bool IsPlaying { get; set; } = false;

        /// <summary>
        /// Gets volume for this channel
        /// </summary>
        public ushort Volume { get; set; } = 100;

        /// <summary>
        /// Gets the music process for this channel
        /// Null when there is no music playing from memory
        /// </summary>
        public MusicProcess MusicProc { get; set; } = null;

    }
}
