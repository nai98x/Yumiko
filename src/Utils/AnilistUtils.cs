namespace Yumiko.Utils
{
    public static class AnilistUtils
    {
        public static DiscordEmbedBuilder GetProfileEmbed(DiscordClient client, User profile)
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
                if (profile.Options.TitleLanguage == "ENGLISH" && !string.IsNullOrEmpty(anime.Title.English))
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
            if (profile.BannerImage != null) embed.WithImageUrl(profile.BannerImage);

            embed.AddField(translations.anime_stats, animeStats, true);
            embed.AddField(translations.manga_stats, mangaStats, true);
            embed.AddField(translations.settings, options, true);

            if (!string.IsNullOrEmpty(favoriteAnime)) embed.AddField($"{DiscordEmoji.FromName(client, ":tv:")} {translations.favorite_animes}", favoriteAnime, true);
            if (!string.IsNullOrEmpty(favoriteManga)) embed.AddField($"{DiscordEmoji.FromName(client, ":book:")} {translations.favorite_mangas}", favoriteManga, true);
            if (!string.IsNullOrEmpty(favoriteCharacters)) embed.AddField($"{DiscordEmoji.FromName(client, ":bust_in_silhouette:")} {translations.favorite_characters}", favoriteCharacters, true);
            if (!string.IsNullOrEmpty(favoriteStaff)) embed.AddField($"{DiscordEmoji.FromName(client, ":man_artist:")} {translations.favorite_staff}", favoriteStaff, true);
            if (!string.IsNullOrEmpty(favoriteStudios)) embed.AddField($"{DiscordEmoji.FromName(client, ":minidisc:")} {translations.favorite_studios}", favoriteStudios, true);

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
            if (!string.IsNullOrEmpty(media.Description)) embed.WithDescription(Common.LimpiarTexto(media.Description).NormalizeDescription());

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

        public static DiscordEmbedBuilder GetMediaUserStatsEmbed(MediaUserStatistics mediaUser)
        {
            var embed = new DiscordEmbedBuilder();

            string notes = !string.IsNullOrEmpty(mediaUser.Notes) ? mediaUser.Notes : translations.without_notes;
            string episodes = $"{mediaUser.Progress}";
            if (mediaUser.Media.Episodes != null) episodes += $"/{mediaUser.Media.Episodes}";
            if (mediaUser.Media.Chapters != null) episodes += $"/{mediaUser.Media.Chapters}";

            embed.WithTitle($"{translations.stats}: {mediaUser.User.Name}");
            embed.WithDescription($"{Formatter.Bold(translations.notes)}\n{notes}".NormalizeDescription());
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

        public static DiscordEmbedBuilder GetRandomCharacterEmbed(InteractionContext ctx, Character character, int page)
        {
            var embed = new DiscordEmbedBuilder();

            embed.WithTitle(character.Name.Full);
            embed.WithUrl(character.SiteUrl);
            embed.WithImageUrl(character.Image.Large);
            embed.WithColor(Constants.YumikoColor);
            embed.WithFooter($"{character.Favorites} {DiscordEmoji.FromName(ctx.Client, ":heart:")} (nº {page} {translations.in_popularity_rank})", Constants.AnilistAvatarUrl);

            if (character.Animes?.Nodes?.Count > 0)
            {
                embed.WithDescription($"{MediaType.ANIME.GetName().UppercaseFirst()}: {Formatter.MaskedUrl(character.Animes.Nodes[0].Title.Romaji, character.Animes.Nodes[0].SiteUrl)}\n");
            }
            else if (character.Mangas?.Nodes?.Count > 0)
            {
                embed.WithDescription($"{MediaType.MANGA.GetName().UppercaseFirst()}: {Formatter.MaskedUrl(character.Mangas.Nodes[0].Title.Romaji, character.Mangas.Nodes[0].SiteUrl)}\n");
            }

            return embed;
        }

        public static DiscordEmbedBuilder GetLoggedProfileEmbed(InteractionContext ctx, User profile)
        {
            var embed = new DiscordEmbedBuilder();

            embed.WithColor(DiscordColor.Green);
            embed.WithTitle(translations.new_profile_saved);
            embed.WithDescription(string.Format(translations.new_profile_saved_mention, ctx.User.Mention));
            embed.WithThumbnail(profile.Avatar.Medium);
            embed.WithAuthor(profile.Name, profile.SiteUrl.AbsoluteUri, ctx.User.AvatarUrl);
            if (!string.IsNullOrEmpty(profile.BannerImage?.AbsoluteUri))
            {
                embed.WithImageUrl(profile.BannerImage.AbsoluteUri);
            }

            return embed;
        }

        public static DiscordEmbedBuilder GetStaffEmbed(Staff staff)
        {
            var embed = new DiscordEmbedBuilder();

            embed.WithColor(Constants.YumikoColor);
            embed.WithTitle(staff.Name.Full);
            if (staff.Description != null) embed.WithDescription(Common.LimpiarTexto(staff.Description).NormalizeDescription());
            embed.WithThumbnail(staff.Image.Large);

            if (staff.Language != null) embed.AddField(translations.language, staff.Language, false);
            if (staff.Age != null) embed.AddField(translations.age, $"{staff.Age}", false);
            if (staff.Gender != null) embed.AddField(translations.gender, staff.Gender, false);
            if (staff.DateOfBirth.Day != null) embed.AddField(translations.date_of_birth, $"{staff.DateOfBirth.Day}/{staff.DateOfBirth.Month}/{staff.DateOfBirth.Year}", true);
            if (staff.DateOfDeath.Day != null) embed.AddField(translations.date_of_death, $"{staff.DateOfDeath.Day}/{staff.DateOfDeath.Month}/{staff.DateOfDeath.Year}", true);

            return embed;
        }

        public static DiscordEmbedBuilder GetMediaRecommendationsEmbed(DiscordUser user, User profile, MediaListCollection collection, MediaType type)
        {
            var embed = new DiscordEmbedBuilder();
            var recommendations = GetRecommendationsFromUser(profile, collection, type);

            if (recommendations.Count == 0)
            {
                embed.WithTitle(translations.error);
                embed.WithDescription(translations.no_recommendations_found);
                embed.WithColor(DiscordColor.Red);
                embed.WithAuthor(profile.Name, profile.SiteUrl.AbsoluteUri, profile.Avatar.Medium.AbsoluteUri);
                embed.WithThumbnail(user.AvatarUrl);
            }
            else
            {
                string desc = string.Join("\n", recommendations.Select(rec => $"{Formatter.Bold($"{rec.Score:##.##}")} - {Formatter.MaskedUrl(rec.Title, new Uri($"https://anilist.co/{type.GetName().ToLower()}/{rec.Id}"))}"));
                string watchedRead = type == MediaType.ANIME ? translations.watched : translations.read;

                embed.WithTitle(string.Format(translations.media_recommendations, type.GetName().UppercaseFirst()));
                embed.WithDescription(desc.NormalizeDescriptionNewLine());
                embed.WithColor(Constants.YumikoColor);
                embed.WithFooter(string.Format(translations.media_recommendations_explanation, watchedRead, type.GetName().ToLower()), Constants.AnilistAvatarUrl);
                embed.WithAuthor(profile.Name, profile.SiteUrl.AbsoluteUri, profile.Avatar.Medium.AbsoluteUri);
                embed.WithThumbnail(user.AvatarUrl);
            }

            return embed;
        }

        public static async Task<List<Anime>> GetRandomMediaListHoL(InteractionContext ctx, List<Anime> listaOriginal, GamemodeHoL gamemode, MediaType mediaType, int min, int max)
        {
            List<Anime> lista = new();
            List<int> valoresBase = new();
            int valorBase = 0;

            do
            {
                valorBase = Common.GetRandomNumber(min, max);
                valoresBase.Add(valorBase);

                var settings = new GameSettings
                {
                    IterIni = valorBase,
                    IterFin = valorBase + 1,
                };

                var listaAux = await GameServices.GetMediaAsync(ctx, mediaType, settings, false, false, false, false, false);
                foreach (var item in listaAux)
                {
                    if ((gamemode == GamemodeHoL.Score && item.AvarageScore > -1) || (gamemode == GamemodeHoL.Popularity && item.Favoritos >= 0))
                    {
                        lista.Add(item);
                    }

                    if (lista.Count >= 500)
                    {
                        return lista;
                    }
                }
            }
            while (valoresBase.Exists(x => x == valorBase));

            return lista;
        }

        public static string FormatScoreUser(string scoreFormat, string scorePers)
        {
            string scoreF = string.Empty;
            switch (scoreFormat)
            {
                case "POINT_10":
                case "POINT_10_DECIMAL":
                    scoreF = $"{scorePers}/10";
                    break;
                case "POINT_100":
                    scoreF = $"{scorePers}/100";
                    break;
                case "POINT_5":
                    int scoreS = int.Parse(scorePers);
                    for (int i = 0; i < scoreS; i++)
                    {
                        scoreF += "★";
                    }

                    break;
                case "POINT_3":
                    int score3 = int.Parse(scorePers);
                    switch (score3)
                    {
                        case 1:
                            scoreF = "🙁";
                            break;
                        case 2:
                            scoreF = "😐";
                            break;
                        case 3:
                            scoreF = "🙂";
                            break;
                    }

                    break;
            }

            return scoreF;
        }

        private static List<AnimeRecommendation> GetRecommendationsFromUser(User profile, MediaListCollection collection, MediaType type)
        {
            List<AnimeRecommendation> recommendations = new();

            decimal meanScore = type == MediaType.ANIME ? profile.Statistics.Anime.MeanScore : profile.Statistics.Manga.MeanScore;
            decimal standardDeviation = type == MediaType.ANIME ? profile.Statistics.Anime.StandardDeviation : profile.Statistics.Manga.StandardDeviation;

            if (standardDeviation == 0)
            {
                return recommendations;
            }

            List<int> mediaListIds = new();
            collection.Lists?.ForEach(list =>
            {
                list.Entries?.ForEach(entry =>
                {
                    mediaListIds.Add(entry.MediaId);
                });
            });

            collection.Lists?.ForEach(list =>
            {
                list.Entries?.ForEach(entry =>
                {
                    if (entry.Score != null && entry.Score > 0) // Filter entries without score
                    {
                        decimal adjustedScore = ((decimal)entry.Score - meanScore) / standardDeviation;
                        entry.Media.Recommendations?.Nodes?.ForEach(node =>
                        {
                            if (node.MediaRecommendation != null) // Filter entries without recommendations
                            {
                                int nodeId = node.MediaRecommendation.Id;
                                string nodeTitle =
                                    (profile.Options.TitleLanguage == "ENGLISH" && node.MediaRecommendation.Title.English != null) ?
                                    node.MediaRecommendation.Title.English : node.MediaRecommendation.Title.Romaji;
                                int nodeRating = node.Rating;

                                if (!mediaListIds.Contains(nodeId) && nodeRating > 0) // Filter entries alredy on list and without rating
                                {
                                    if (!recommendations.Where(x => x.Id == nodeId).Any())
                                    {
                                        recommendations.Add(new()
                                        {
                                            Id = nodeId,
                                            Title = nodeTitle
                                        });
                                    }

                                    recommendations.First(x => x.Id == nodeId).Score += adjustedScore * (2 - (1 / nodeRating));
                                }
                            }
                        });
                    }
                });
            });

            return recommendations.OrderByDescending(x => x.Score).Where(y => y.Score >= 3).ToList();
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
