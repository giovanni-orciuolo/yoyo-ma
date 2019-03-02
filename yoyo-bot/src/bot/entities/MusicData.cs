using DSharpPlus.Entities;

namespace yoyo_bot.src.bot.entities
{
    /// <summary>
    /// Marks different types of music
    /// </summary>
    enum MusicTypes
    {
        MEMORY,
        YOUTUBE
    }

    /// <summary>
    /// Represents a song currently getting played
    /// </summary>
    class MusicData
    {
        /// <summary>
        /// Music source (path or link or w/e)
        /// Prefer using GetFormattedSource instead of accessing this
        /// when displaying the value
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Music channel associated with this data
        /// </summary>
        public GuildMusicChannel Channel { get; set; }

        /// <summary>
        /// User that requested this music
        /// </summary>
        public DiscordMember RequestedBy { get; set; }

        /// <summary>
        /// Music type for this data
        /// </summary>
        public MusicTypes MusicType { get; set; }

        /// <summary>
        /// Returns the source in a proper formatted way
        /// </summary>
        /// <returns></returns>
        public string GetFormattedSource()
        {
            switch (this.MusicType)
            {
                case MusicTypes.MEMORY: return this.Source.Substring(this.Source.LastIndexOf(@"\") + 1);
                case MusicTypes.YOUTUBE: return this.Source;
                default: return this.Source;
            }
        }
    }
}
