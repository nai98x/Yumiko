namespace Yumiko.Utils
{
    public static class Constants
    {
        public const string AnilistAvatarUrl = @"https://anilist.co/img/icons/android-chrome-512x512.png";

        public const string AnilistAPIUrl = @"https://graphql.anilist.co";

        public const string AniThemesAPIUrl = @"https://api.animethemes.moe";

        public const int AnilistPerPage = 25;

        public const int QueryDelay = 500;

        public static DiscordColor YumikoColor { get; private set; } = DiscordColor.Blurple;
    }
}
