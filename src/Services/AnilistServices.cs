namespace Yumiko.Services
{
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static class AnilistServices
    {
        private static readonly GraphQLHttpClient GraphQlClient = new("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        public static async Task<Media> GetAniListMedia(InteractionContext ctx, double timeoutGeneral, string busqueda, MediaType type)
        {
            string query = "query($busqueda : String){" +
            "   Page(perPage:5){" +
            "       media(type: " + type.GetName() + ", search: $busqueda){" +
            "           id," +
            "           title{" +
            "               romaji" +
            "           }," +
            "           coverImage{" +
            "               large" +
            "           }," +
            "           siteUrl," +
            "           description," +
            "           format," +
            "           chapters" +
            "           episodes" +
            "           status," +
            "           meanScore," +
            "           genres," +
            "           seasonYear," +
            "           startDate{" +
            "               year," +
            "               month," +
            "               day" +
            "           }," +
            "           endDate{" +
            "               year," +
            "               month," +
            "               day" +
            "           }," +
            "           genres," +
            "           tags{" +
            "               name," +
            "               isMediaSpoiler" +
            "           }," +
            "           synonyms," +
            "           studios{" +
            "               nodes{" +
            "                   name," +
            "                   siteUrl" +
            "               }" +
            "           }," +
            "           externalLinks{" +
            "               site," +
            "               url" +
            "           }," +
            "           isAdult" +
            "       }" +
            "   }" +
            "}";

            var request = new GraphQLRequest
            {
                Query = query,
                Variables = new
                {
                    busqueda,
                },
            };

            try
            {
                var data = await GraphQlClient.SendQueryAsync<dynamic>(request);
                if (data.Data != null)
                {
                    if (data.Data.Page.media != null && data.Data.Page.media.Count > 0)
                    {
                        int cont = 0;
                        List<AnimeShort> opc = new();
                        foreach (var animeP in data.Data.Page.media)
                        {
                            cont++;
                            string opcTitle = animeP.title.romaji;
                            string opcFormat = animeP.format;
                            string opcYear = animeP.seasonYear;
                            string desc = opcFormat;
                            if (!string.IsNullOrEmpty(opcYear))
                            {
                                desc += $" ({opcYear})";
                            }

                            opc.Add(new AnimeShort
                            {
                                Title = opcTitle,
                                Description = $"{desc}",
                            });
                        }

                        var elegido = await Common.GetElegidoAsync(ctx, timeoutGeneral, opc);
                        if (elegido > 0)
                        {
                            var datos = data.Data.Page.media[elegido - 1];
                            return DecodeMedia(datos);
                        }
                        else
                        {
                            return new()
                            {
                                Ok = false,
                                MsgError = translations.response_timed_out,
                            };
                        }
                    }
                }

                return new()
                {
                    Ok = false,
                    MsgError = $"{string.Format(translations.not_found, type.GetName().ToLower())}: `{busqueda}`",
                };
            }
            catch (Exception e)
            {
                await Common.GrabarLogErrorAsync(ctx, $"Error in AnilistUtils - GetAnilistMedia, type: {type.GetName()}\nError: {e.Message}");
                return new()
                {
                    Ok = false,
                    MsgError = $"{e.Message}",
                };
            }
        }

        public static async Task<(string, bool)> GetAniListMediaTitleAndNsfwFromId(InteractionContext ctx, int id, MediaType type)
        {
            string query = "query($id : Int){" +
            "   Media(type: " + type.GetName() + ", id: $id){" +
            "       title{" +
            "           romaji" +
            "       }," +
            "       isAdult" +
            "   }" +
            "}";

            var request = new GraphQLRequest
            {
                Query = query,
                Variables = new
                {
                    id,
                },
            };

            try
            {
                var data = await GraphQlClient.SendQueryAsync<dynamic>(request);
                if (data.Data != null)
                {
                    if (data.Data.Media != null)
                    {
                        var datos = data.Data.Media;
                        string nombreMedia = datos.title.romaji;
                        string nsfw = datos.isAdult;
                        return (nombreMedia, bool.Parse(nsfw));
                    }
                }

                return (string.Empty, false);
            }
            catch (Exception e)
            {
                await Common.GrabarLogErrorAsync(ctx, $"Error in AnilistUtils - GetAniListMediaTitleFromId, type: {type.GetName()}\nError: {e.Message}");
                return (string.Empty, false);
            }
        }

        public static Media? DecodeMedia(dynamic datos)
        {
            if (datos != null)
            {
                string idStr = datos.id;
                string isadult = datos.isAdult;

                Media media = new();

                media.Ok = true;
                media.Id = int.Parse(idStr);
                media.IsAdult = bool.Parse(isadult);
                media.Descripcion = datos.description;
                media.Descripcion = Common.NormalizarDescription(Common.LimpiarTexto(media.Descripcion));
                if (media.Descripcion == string.Empty)
                {
                    media.Descripcion = translations.without_description;
                }

                media.Estado = datos.status;
                media.Episodios = datos.episodes;
                media.Chapters = datos.chapters;
                media.Formato = datos.format;
                media.Score = $"{datos.meanScore}/100";
                media.Generos = string.Empty;
                foreach (var genero in datos.genres)
                {
                    media.Generos += genero;
                    media.Generos += ", ";
                }

                if (media.Generos.Length >= 2)
                {
                    media.Generos = media.Generos.Remove(media.Generos.Length - 2);
                }

                media.Tags = string.Empty;
                foreach (var tag in datos.tags)
                {
                    if (tag.isMediaSpoiler == "false")
                    {
                        media.Tags += tag.name;
                    }
                    else
                    {
                        media.Tags += $"||{tag.name}||";
                    }

                    media.Tags += ", ";
                }

                if (media.Tags.Length >= 2)
                {
                    media.Tags = media.Tags.Remove(media.Tags.Length - 2);
                }

                media.Titulos = new();
                foreach (string title in datos.synonyms)
                {
                    media.Titulos.Add(title);
                }

                media.Estudios = string.Empty;
                var nodos = datos.studios.nodes;
                if (nodos.HasValues)
                {
                    foreach (var studio in datos.studios.nodes)
                    {
                        media.Estudios += $"[{studio.name}]({studio.siteUrl}), ";
                    }
                }

                if (media.Estudios.Length >= 2)
                {
                    media.Estudios = media.Estudios.Remove(media.Estudios.Length - 2);
                }

                media.LinksExternos = string.Empty;
                foreach (var external in datos.externalLinks)
                {
                    media.LinksExternos += $"[{external.site}]({external.url}), ";
                }

                if (media.LinksExternos.Length >= 2)
                {
                    media.LinksExternos = media.LinksExternos.Remove(media.LinksExternos.Length - 2);
                }

                if (datos.startDate.day != null)
                {
                    if (datos.endDate.day != null)
                    {
                        media.Fechas = $"{datos.startDate.day}/{datos.startDate.month}/{datos.startDate.year} al {datos.endDate.day}/{datos.endDate.month}/{datos.endDate.year}";
                    }
                    else
                    {
                        media.Fechas = string.Format(translations.airing_since, $"{datos.startDate.day}/{datos.startDate.month}/{datos.startDate.year}");
                    }
                }
                else
                {
                    media.Fechas = translations.no_air_date;
                }

                media.TituloRomaji = datos.title.romaji;
                media.UrlAnilist = datos.siteUrl;
                media.CoverImage = datos.coverImage.large;

                return media;
            }
            else
            {
                return null;
            }
        }

        public static async Task<DiscordEmbedBuilder?> GetInfoMediaUser(InteractionContext ctx, int anilistId, int mediaId)
        {
            var requestPers = new GraphQLRequest
            {
                Query =
                    @"query ($codigoal: Int, $codigome: Int) {
                        MediaList(userId: $codigoal, mediaId: $codigome){
                            status,
                            progress,
                            startedAt {
                                year,
                                month,
                                day
                            },
                            completedAt {
                                year,
                                month,
                                day
                            },
                            notes,
                            score,
                            repeat,
                            media {
                                episodes,
                                chapters
                            },
                            user {
                                name,
                                avatar {
                                    large
                                },
                                mediaListOptions {
                                    scoreFormat
                                }
                            }
                        }
                    }",
                Variables = new
                {
                    codigoal = anilistId,
                    codigome = mediaId,
                },
            };
            try
            {
                var data = await GraphQlClient.SendQueryAsync<dynamic>(requestPers);
                if (data.Data != null)
                {
                    dynamic datos = data.Data.MediaList;
                    string status = datos.status;
                    string progress = datos.progress;
                    string episodiosMedia = datos.media.episodes;
                    string chaptersMedia = datos.media.chapters;
                    string scorePers = datos.score;
                    string startedd = datos.startedAt.day;
                    string startedm = datos.startedAt.month;
                    string startedy = datos.startedAt.year;
                    string completedd = datos.completedAt.day;
                    string completedm = datos.completedAt.month;
                    string completedy = datos.completedAt.year;
                    string notas = datos.notes;
                    string rewatches = datos.repeat;
                    string scoreFormat = datos.user.mediaListOptions.scoreFormat;
                    string nameAl = datos.user.name;
                    string avatarAl = datos.user.avatar.large;

                    if (string.IsNullOrEmpty(notas))
                    {
                        notas = translations.without_notes;
                    }

                    var builderPers = new DiscordEmbedBuilder
                    {
                        Title = $"{translations.stats}: {nameAl}",
                        Description = Common.NormalizarDescription($"**{translations.notes}**\n" + notas),
                        Color = Constants.YumikoColor,
                    }.WithThumbnail(avatarAl);

                    builderPers.AddField(translations.status, status, true);
                    if (!string.IsNullOrEmpty(progress))
                    {
                        string episodios = progress;
                        if (!string.IsNullOrEmpty(episodiosMedia))
                        {
                            episodios += $"/{episodiosMedia}";
                        }

                        if (!string.IsNullOrEmpty(chaptersMedia))
                        {
                            episodios += $"/{chaptersMedia}";
                        }

                        builderPers.AddField(translations.episodes, episodios, true);
                    }

                    string scoreMostrar = string.Empty;
                    if (!string.IsNullOrEmpty(scorePers) && !string.IsNullOrEmpty(scoreFormat) && scorePers != "0")
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

                        builderPers.AddField(translations.score, scoreF, true);
                    }
                    else
                    {
                        builderPers.AddField(translations.score, translations.not_assigned, true);
                    }

                    if (!string.IsNullOrEmpty(rewatches))
                    {
                        builderPers.AddField("Rewatches", $"{rewatches}", false);
                    }

                    if (!string.IsNullOrEmpty(startedd) && !string.IsNullOrEmpty(startedm) && !string.IsNullOrEmpty(startedy))
                    {
                        builderPers.AddField(translations.start_date, $"{startedd}/{startedm}/{startedy}", true);
                    }

                    if (!string.IsNullOrEmpty(completedd) && !string.IsNullOrEmpty(completedm) && !string.IsNullOrEmpty(completedy))
                    {
                        builderPers.AddField(translations.end_date, $"{completedd}/{completedm}/{completedy}", true);
                    }

                    return builderPers;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != "The HTTP request failed with status code NotFound")
                {
                    await Common.GrabarLogErrorAsync(ctx, $"Error in GetPersMedia: {ex.Message}\n```{ex.StackTrace}```");
                }
            }

            return null;
        }

        public static async Task<DiscordEmbedBuilder?> GetUserRecommendationsAsync(InteractionContext ctx, MediaType type, int anilistUserId)
        {
            var requestPers = new GraphQLRequest
            {
                Query =
                    "query ($id: Int) {" +
                    "   User(id: $id) {" +
                    "       options {" +
                    "           titleLanguage" +
                    "       }," +
                    "       statistics {" +
                    "           anime {" +
                    "               meanScore," +
                    "               standardDeviation" +
                    "           }," +
                    "           manga {" +
                    "               meanScore," +
                    "               standardDeviation" +
                    "           }" +
                    "       }" +
                    "   }" +
                    "   MediaListCollection(userId: $id, type: " + type.GetName() + ", status_not_in: [PLANNING], forceSingleCompletedList: true) {" +
                    "       lists {" +
                    "           entries {" +
                    "               mediaId," +
                    "               score(format: POINT_100)," +
                    "               status" +
                    "               media {" +
                    "                   recommendations(sort: RATING_DESC, perPage: 5) {" +
                    "                       nodes {" +
                    "                           rating," +
                    "                           mediaRecommendation {" +
                    "                               id," +
                    "                               title {" +
                    "                                   romaji," +
                    "                                   english" +
                    "                               }" +
                    "                           }" +
                    "                       }" +
                    "                   }" +
                    "               }" +
                    "           }" +
                    "       }" +
                    "   }" +
                    "}",
                Variables = new
                {
                    id = anilistUserId
                }
            };
            try
            {
                var data = await GraphQlClient.SendQueryAsync<dynamic>(requestPers);
                if (data.Data != null)
                {
                    List<AnimeRecommendation> recommendations = new();
                    dynamic userData = data.Data.User;
                    string titleLanguage = userData.options.titleLanguage;
                    decimal meanScore;
                    decimal standardDeviation;
                    switch (type)
                    {
                        case MediaType.ANIME:
                            meanScore = userData.statistics.anime.meanScore;
                            standardDeviation = userData.statistics.anime.standardDeviation;
                            break;
                        case MediaType.MANGA:
                            meanScore = userData.statistics.manga.meanScore;
                            standardDeviation = userData.statistics.manga.standardDeviation;
                            break;
                        default:
                            throw new ArgumentException("Programming error");
                    }

                    dynamic mediaListsData = data.Data.MediaListCollection.lists;

                    List<int> mediaListIds = new();
                    foreach (var list in mediaListsData)
                    {
                        foreach (var entry in list.entries)
                        {
                            int id = entry.mediaId;
                            mediaListIds.Add(id);
                        }
                    }

                    foreach (var list in mediaListsData)
                    {
                        foreach (var entry in list.entries)
                        {
                            decimal mediaScore = entry.score;
                            if (mediaScore > 0) // Filter entries without score
                            {
                                decimal adjustedScore = (mediaScore - meanScore) / standardDeviation;
                                foreach (var node in entry.media.recommendations.nodes)
                                {
                                    if (node.mediaRecommendation != null && node.mediaRecommendation.id != null) // Filter entries without recommendations
                                    {
                                        int nodeId = node.mediaRecommendation.id;
                                        string nodeTitle;
                                        if (titleLanguage == "ENGLISH" && node.mediaRecommendation.title.english != null)
                                        {
                                            nodeTitle = node.mediaRecommendation.title.english;
                                        }
                                        else
                                        {
                                            nodeTitle = node.mediaRecommendation.title.romaji;
                                        }
                                        
                                        int nodeRating = node.rating;
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

                                            var rec = recommendations.First(x => x.Id == nodeId);
                                            rec.Score += adjustedScore * (2 - 1 / nodeRating);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var sorted = recommendations.OrderByDescending(x => x.Score).Take(25).Where(y => y.Score >= 3).ToList();

                    if (sorted.Count == 0)
                    {
                        return new DiscordEmbedBuilder
                        {
                            Title = translations.error,
                            Description = translations.no_recommendations_found,
                            Color = DiscordColor.Red
                        };
                    }

                    string desc = string.Empty;
                    foreach (var item in sorted)
                    {
                        desc += $"**{item.Score:##.##}** - [{item.Title}](https://anilist.co/{type.GetName().ToLower()}/{item.Id}/)\n";
                    }

                    return new DiscordEmbedBuilder
                    {
                        Title = string.Format(translations.media_recommendations, type.GetName().UppercaseFirst(), ctx.User.FullName()),
                        Description = desc,
                        Color = Constants.YumikoColor
                    };
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != "The HTTP request failed with status code NotFound")
                {
                    await Common.GrabarLogErrorAsync(ctx, $"Error in GetUserRecommendationsAsync: {ex.Message}\n```{ex.StackTrace}```");
                }
            }

            return null;
        }
    }
}
