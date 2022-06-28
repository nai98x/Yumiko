namespace Yumiko.Utils
{
    public static class Constants
    {
        public const string AnilistAvatarUrl = @"https://anilist.co/img/icons/android-chrome-512x512.png";

        public const string AnilistAPIUrl = @"https://graphql.anilist.co";

        public const int AnilistPerPage = 25;

        public static DiscordColor YumikoColor { get; private set; } = DiscordColor.Blurple;

        public static DiscordEmbedBuilder NsfwWarning { get; private set; } = new DiscordEmbedBuilder
        {
            Title = "NSFW Required",
            Description = "This command must be used on an NSFW channel.",
            Color = new DiscordColor(0xFF0000),
        };
    }
}
