namespace Yumiko.Utils
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

        public static DiscordEmbedBuilder GetMediaEmbed(InteractionContext ctx, Media media, MediaType type)
        {
            var embed = new DiscordEmbedBuilder();

            string dates = string.Empty;
            if (media.StartDate.Day != null) dates += $"{Formatter.Bold(translations.from)}: {media.StartDate.Day}/{media.StartDate.Month}/{media.StartDate.Year}\n";
            if (media.EndDate.Day != null) dates += $"{Formatter.Bold(translations.to)}: {media.EndDate.Day}/{media.EndDate.Month}/{media.EndDate.Year}";
            string genres = (media.Genres != null && media.Genres.Count > 0) ? string.Join(", ", media.Genres) : string.Empty;
            string tags = (media.Tags != null && media.Tags.Count > 0) ? string.Join(", ", media.Tags.Select(tag => tag.IsMediaSpoiler ? $"||{tag.Name}||" : tag.Name)) : string.Empty;
            string titles = media.Synonyms?.Count > 0 ? string.Join(", ", media.Synonyms) : translations.without_titles;
            string studios = media.Studios?.Nodes?.Count > 0 ? string.Join(", ", media.Studios.Nodes.Select(studio => Formatter.MaskedUrl(studio.Name, studio.SiteUrl))) : string.Empty;
            string externalLinks = media.ExternalLinks?.Count > 0 ? string.Join(", ", media.ExternalLinks.Select(link => Formatter.MaskedUrl(link.Site, link.Url))) : string.Empty;

            embed.WithTitle(media.Title.Romaji);
            embed.WithUrl(media.SiteUrl);
            embed.WithThumbnail(media.CoverImage.Large);
            embed.WithColor(Constants.YumikoColor);
            if (media.BannerImage != null) embed.WithImageUrl(media.BannerImage);
            if (!string.IsNullOrEmpty(media.Description)) embed.WithDescription(Common.LimpiarTexto(media.Description));

            if (type == MediaType.ANIME && media.Episodes != null) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":1234:")} {translations.episodes}", $"{media.Episodes}", true);
            if (type == MediaType.MANGA && media.Chapters != null) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":1234:")} {translations.chapters}", $"{media.Chapters}", true);
            if (media.Format != null) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":dividers:")} {translations.format}", $"{Enum.GetName(typeof(MediaFormat), media.Format)!.UppercaseFirst()}", true);
            if (media.Status != null) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":hourglass_flowing_sand:")} {translations.status}", $"{Enum.GetName(typeof(MediaStatus), media.Status)!.UppercaseFirst()}", true);
            if (media.MeanScore != null) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":star:")} {translations.score}", $"{media.MeanScore}", false);
            if (!string.IsNullOrEmpty(dates)) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":calendar_spiral:")} {translations.start_date}", dates, false);
            if (!string.IsNullOrEmpty(genres)) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":scroll:")} {translations.genres}", genres.NormalizeField(), false);
            if (!string.IsNullOrEmpty(tags)) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":notepad_spiral:")} {translations.tags}", tags.NormalizeField(), false);
            if (!string.IsNullOrEmpty(titles)) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":pencil:")} {translations.synonyms}", titles.NormalizeField(), false);
            if (!string.IsNullOrEmpty(studios)) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":minidisc:")} {translations.studios}", studios.NormalizeField(), false);
            if (!string.IsNullOrEmpty(externalLinks)) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":link:")} {translations.external_links}", externalLinks.NormalizeField(), false);

            return embed;
        }

        public static DiscordEmbedBuilder GetMediaUserStats(MediaUser mediaUser)
        {
            var embed = new DiscordEmbedBuilder();

            string notes = !string.IsNullOrEmpty(mediaUser.Notes) ? mediaUser.Notes : translations.without_notes;
            string episodes = $"{mediaUser.Progress}";
            if (mediaUser.Media.Episodes != null) episodes += $"/{mediaUser.Media.Episodes}";
            if (mediaUser.Media.Chapters != null) episodes += $"/{mediaUser.Media.Chapters}";

            embed.WithTitle($"{translations.stats}: {mediaUser.User.Name}");
            embed.WithDescription($"{Formatter.Bold(translations.notes)}\n{notes}");
            embed.WithColor(Constants.YumikoColor);
            embed.WithThumbnail(mediaUser.User.Avatar.Medium);
            embed.WithUrl(mediaUser.User.SiteUrl);

            embed.AddField(translations.status, Enum.GetName(typeof(MediaListStatus), mediaUser.Status)!.UppercaseFirst(), true);
            embed.AddField(mediaUser.Media.Episodes != null ? translations.episodes : translations.chapters, $"{episodes}", true);
            embed.AddField(translations.score, mediaUser.Score > 0 ? FormatScore(mediaUser.Score, mediaUser.User.MediaListOptions.ScoreFormat) : translations.not_assigned, true);
            embed.AddField("Rewatches", $"{mediaUser.Repeat}", false);
            if (mediaUser.StartedAt.Day != null) embed.AddField(translations.start_date, $"{mediaUser.StartedAt.Day}/{mediaUser.StartedAt.Month}/{mediaUser.StartedAt.Year}", true);
            if (mediaUser.CompletedAt.Day != null) embed.AddField(translations.end_date, $"{mediaUser.CompletedAt.Day}/{mediaUser.CompletedAt.Month}/{mediaUser.CompletedAt.Year}", true);

            return embed;
        }

        public static DiscordEmbedBuilder GetCharacterEmbed(InteractionContext ctx, Character character)
        {
            var embed = new DiscordEmbedBuilder();

            string animes = (character.Animes.Nodes != null && character.Animes.Nodes.Count > 0) ? string.Join("\n", character.Animes.Nodes.Select(anime => Formatter.MaskedUrl(anime.Title.Romaji, anime.SiteUrl))) : string.Empty;
            string mangas = (character.Mangas.Nodes != null && character.Mangas.Nodes.Count > 0) ? string.Join("\n", character.Mangas.Nodes.Select(manga => Formatter.MaskedUrl(manga.Title.Romaji, manga.SiteUrl))) : string.Empty;

            embed.WithTitle(character.Name.Full);
            embed.WithUrl(character.SiteUrl);
            embed.WithThumbnail(character.Image.Large);
            embed.WithColor(Constants.YumikoColor);
            if (!string.IsNullOrEmpty(character.Description)) embed.WithDescription(Common.LimpiarTexto(character.Description).NormalizeDescription());

            if (!string.IsNullOrEmpty(animes)) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":tv:")} Animes", animes.NormalizeField(), false);
            if (!string.IsNullOrEmpty(mangas)) embed.AddField($"{DiscordEmoji.FromName(ctx.Client, ":book:")} Mangas", mangas.NormalizeField(), false);

            return embed;
        }

        private static string FormatScore(decimal score, ScoreFormat format)
        {
            switch (format)
            {
                case ScoreFormat.POINT_10:
                case ScoreFormat.POINT_10_DECIMAL:
                    return $"{score}/10";
                case ScoreFormat.POINT_100:
                    return $"{score}/100";
                case ScoreFormat.POINT_5:
                    string score5 = string.Empty;
                    for (int i = 0; i < score; i++)
                    {
                        score5 += "★";
                    }
                    return score5;
                case ScoreFormat.POINT_3:
                    return score switch
                    {
                        1 => "🙁",
                        2 => "😐",
                        3 => "🙂",
                        _ => throw new ArgumentOutOfRangeException(nameof(score)),
                    };
                default:
                    throw new ArgumentException("Invalid ScoreFormat type");
            }
        }
    }
}
