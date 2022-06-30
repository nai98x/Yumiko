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

        public static async Task<DiscordEmbedBuilder?> GetUserRecommendationsAsync(InteractionContext ctx, DiscordUser user, MediaType type, int anilistUserId)
        {
            var requestPers = new GraphQLRequest
            {
                Query =
                    "query ($id: Int) {" +
                    "   User(id: $id) {" +
                    "       name," +
                    "       avatar {" +
                    "           medium" +
                    "       }," +
                    "       siteUrl," +
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
                    string userName = userData.name;
                    string userAvatar = userData.avatar.medium;
                    string userUrl = userData.siteUrl;
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
                                            rec.Score += adjustedScore * (2 - (1 / nodeRating));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var sorted = recommendations.OrderByDescending(x => x.Score).Where(y => y.Score >= 3).ToList();

                    if (sorted.Count == 0)
                    {
                        return new DiscordEmbedBuilder
                        {
                            Title = translations.error,
                            Description = translations.no_recommendations_found,
                            Color = DiscordColor.Red,
                            Author = new()
                            {
                                IconUrl = userAvatar,
                                Name = userName,
                                Url = userUrl
                            },
                            Thumbnail = new()
                            {
                                Url = user.AvatarUrl
                            }
                        };
                    }

                    string desc = string.Empty;
                    foreach (var item in sorted)
                    {
                        desc += $"**{item.Score:##.##}** - [{item.Title}](https://anilist.co/{type.GetName().ToLower()}/{item.Id}/)\n";
                    }

                    return new DiscordEmbedBuilder
                    {
                        Title = string.Format(translations.media_recommendations, type.GetName().UppercaseFirst()),
                        Description = desc.NormalizeDescriptionNewLine(),
                        Color = Constants.YumikoColor,
                        Footer = new()
                        {
                            Text = string.Format(translations.media_recommendations_explanation, type.GetName().ToLower()),
                            IconUrl = Constants.AnilistAvatarUrl
                        },
                        Author = new()
                        {
                            IconUrl = userAvatar,
                            Name = userName,
                            Url = userUrl
                        },
                        Thumbnail = new()
                        {
                            Url = user.AvatarUrl
                        }
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
