﻿namespace Yumiko.Utils
{
    public static class AnilistUtils
    {
        public static DiscordEmbedBuilder GetProfileEmbed(InteractionContext ctx, Profile profile)
        {
            var embed = new DiscordEmbedBuilder();

            string animeStats = 
                $"{translations.total}: `{profile.Statistics.Anime.Count}`\n" +
                $"{translations.episodes}: `{profile.Statistics.Anime.EpisodesWatched}`\n" +
                $"{translations.mean_score}: `{profile.Statistics.Anime.MeanScore}`";
            string mangaStats =
                $"{translations.total}: `{profile.Statistics.Manga.Count}`\n" +
                $"{translations.chapters}: `{profile.Statistics.Manga.ChaptersRead}`\n" +
                $"{translations.mean_score}: `{profile.Statistics.Manga.MeanScore}`";
            string options = 
                $"{translations.titles_language}: `{profile.Options.TitleLanguage.UppercaseFirst()}`\n" +
                $"{translations.adult_content}: {profile.Options.DisplayAdultContent.ToYesNo()}\n" +
                $"{translations.color}: {profile.Options.ProfileColor}";

            string favoriteAnime = string.Empty;
            profile.Favourites.Anime.Nodes?.ForEach(anime =>
            {
                if(profile.Options.TitleLanguage == "ENGLISH" && !string.IsNullOrEmpty(anime.Title.English))
                {
                    favoriteAnime += $"{Formatter.MaskedUrl(anime.Title.English, anime.SiteUrl)}\n";
                }
                else
                {
                    favoriteAnime += $"{Formatter.MaskedUrl(anime.Title.Romaji, anime.SiteUrl)}\n";
                }
            });

            string favoriteManga = string.Empty;
            profile.Favourites.Manga.Nodes?.ForEach(manga =>
            {
                if (profile.Options.TitleLanguage == "ENGLISH" && !string.IsNullOrEmpty(manga.Title.English))
                {
                    favoriteManga += $"{Formatter.MaskedUrl(manga.Title.English, manga.SiteUrl)}\n";
                }
                else
                {
                    favoriteManga += $"{Formatter.MaskedUrl(manga.Title.Romaji, manga.SiteUrl)}\n";
                }
            });

            string favoriteCharacters = string.Empty;
            profile.Favourites.Characters.Nodes?.ForEach(character =>
            {
                favoriteCharacters += $"{Formatter.MaskedUrl(character.Name.Full, character.SiteUrl)}\n";
            });

            string favoriteStaff = string.Empty;
            profile.Favourites.Staff.Nodes?.ForEach(staff =>
            {
                favoriteStaff += $"{Formatter.MaskedUrl(staff.Name.Full, staff.SiteUrl)}\n";
            });

            string favoriteStudios = string.Empty;
            profile.Favourites.Studios.Nodes?.ForEach(studio =>
            {
                favoriteStudios += $"{Formatter.MaskedUrl(studio.Name, studio.SiteUrl)}\n";
            });

            embed.WithTitle(profile.Name);
            embed.WithThumbnail(profile.Avatar.Medium);
            embed.WithColor(Constants.YumikoColor);
            embed.WithImageUrl(profile.BannerImage);

            embed.AddField(translations.anime_stats, animeStats, true);
            embed.AddField(translations.manga_stats, mangaStats, true);
            embed.AddField(translations.settings, options, true);

            if (!string.IsNullOrEmpty(favoriteAnime)) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":tv:")} {translations.favorite_animes}", favoriteAnime, true);
            if (!string.IsNullOrEmpty(favoriteManga)) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":book:")} {translations.favorite_mangas}", favoriteManga, true);
            if (!string.IsNullOrEmpty(favoriteCharacters)) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":bust_in_silhouette:")} {translations.favorite_characters}", favoriteCharacters, true);
            if (!string.IsNullOrEmpty(favoriteStaff)) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":man_artist:")} {translations.favorite_staff}", favoriteStaff, true);
            if (!string.IsNullOrEmpty(favoriteStudios)) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":minidisc:")} {translations.favorite_studios}", favoriteStudios, true);

            return embed;
        }
    }
}