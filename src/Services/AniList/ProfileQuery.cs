namespace Yumiko.Services
{
    using DSharpPlus.SlashCommands;
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public static class ProfileQuery
    {
        private static readonly GraphQLHttpClient GraphQlClient = new(Constants.AnilistAPIUrl, new NewtonsoftJsonSerializer());

        public static async Task<Profile?> GetProfile(InteractionContext ctx, int userId)
        {
            try
            {
                var request = new GraphQLRequest
                {
                    Query = searchQuery,
                    Variables = new
                    {
                        code = userId
                    }
                };
                var response = await GraphQlClient.SendQueryAsync<ProfileResponse>(request);
                var result = response.Data.User;

                return result;
            }
            catch (GraphQLHttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound) return null;
                await Common.GrabarLogErrorAsync(ctx, $"Unknown error: {ex.StatusCode}: {ex.Message}\n{Formatter.BlockCode(ex.StackTrace)}");
                return null;
            }
            catch (Exception e)
            {
                await Common.GrabarLogErrorAsync(ctx, $"Unknown error: {e.Message}\n{Formatter.BlockCode(e.StackTrace)}");
                return null;
            }
        }

        private const string searchQuery = @"
            query ($code: Int){
                User(id: $code) {
                    id
                    name
                    siteUrl
                    avatar {
                        medium
                    }
                    bannerImage
                    options {
                        titleLanguage
                        displayAdultContent
                        profileColor
                    }
                    statistics {
                        anime {
                            count
                            episodesWatched
                            meanScore
                        }
                        manga {
                            count
                            chaptersRead
                            meanScore
                        }
                    }
                    favourites {
                        anime(perPage: 3) {
                            nodes {
                                title {
                                    romaji
                                    english
                                    native
                                }
                                siteUrl
                            }
                        }
                        manga(perPage: 3) {
                            nodes {
                                title {
                                    romaji
                                    english
                                    native
                                }
                                siteUrl
                            }
                        }
                        characters(perPage: 3) {
                            nodes {
                                name {
                                    full
                                }
                                siteUrl
                            }
                        }
                        staff(perPage: 3) {
                            nodes {
                                name {
                                    full
                                }
                                siteUrl
                            }
                        }
                        studios(perPage: 3) {
                            nodes {
                                name
                                siteUrl
                                isAnimationStudio
                            }
                        }
                    }
                }
            }
        ";
    }
}
