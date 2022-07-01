namespace Yumiko.Services
{
    using DSharpPlus.SlashCommands;
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public static class RecommendatiosnQuery
    {
        private static readonly GraphQLHttpClient GraphQlClient = new(Constants.AnilistAPIUrl, new NewtonsoftJsonSerializer());

        public static async Task<(Profile?, MediaListCollection?)> GetRecommendations(InteractionContext ctx, int userId, MediaType mediaType)
        {
            try
            {
                var request = new GraphQLRequest
                {
                    Query = searchQuery,
                    Variables = new
                    {
                        userId,
                        type = Enum.GetName(typeof(MediaType), mediaType)
                    }
                };
                var response = await GraphQlClient.SendQueryAsync<RecommendationsResponse>(request);
                var user = response.Data.User;
                var recommendations = response.Data.Recommendations;

                return (user, recommendations);
            }
            catch (GraphQLHttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound) return (null, null);
                await Common.GrabarLogErrorAsync(ctx, $"Unknown error: {ex.StatusCode}: {ex.Message}\n{Formatter.BlockCode(ex.StackTrace)}");
                return (null, null);
            }
            catch (Exception e)
            {
                await Common.GrabarLogErrorAsync(ctx, $"Unknown error: {e.Message}\n{Formatter.BlockCode(e.StackTrace)}");
                return (null, null);
            }
        }

        private const string searchQuery = @"
            query($userId: Int, $type: MediaType) {
                User(id: $userId) {
                    name
                    avatar {
                        medium
                    }
                    siteUrl
                    options {
                        titleLanguage
                    }
                    statistics {
                        anime {
                            meanScore
                            standardDeviation
                        }
                        manga {
                            meanScore
                            standardDeviation
                        }
                    }
                }
                MediaListCollection(userId: $userId, type: $type, status_not_in: [PLANNING], forceSingleCompletedList: true) {
                    lists {
                        entries {
                            mediaId
                            score(format: POINT_100)
                            status
                            media {
                                recommendations(sort: RATING_DESC, perPage: 5) {
                                    nodes {
                                        rating
                                        mediaRecommendation {
                                            id
                                            title {
                                                romaji
                                                english
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        ";
    }
}
