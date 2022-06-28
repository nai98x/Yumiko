namespace Yumiko.Services
{
    using DSharpPlus.SlashCommands;
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public static class MediaQuery
    {
        private static readonly GraphQLHttpClient GraphQlClient = new("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        //public static async Task<Media> GetAniListMedia(InteractionContext ctx, double timeoutGeneral, string busqueda, MediaType type)
        //public static async Task<(string, bool)> GetAniListMediaTitleAndNsfwFromId(InteractionContext ctx, int id, MediaType type)
        //public static Media? DecodeMedia(dynamic datos)
        //public static async Task<DiscordEmbedBuilder?> GetInfoMediaUser(InteractionContext ctx, int anilistId, int mediaId)
        //public static async Task<DiscordEmbedBuilder?> GetUserRecommendationsAsync(InteractionContext ctx, DiscordUser user, MediaType type, int anilistUserId)

        public static async Task<Media> GetMedia(InteractionContext ctx, string mediaSearch, MediaType mediaType)
        {
            try
            {
                var request = new GraphQLRequest
                {
                    Query = searchMediaQuery,
                    Variables = new
                    {
                        search = mediaSearch,
                        type = Enum.GetName(typeof(MediaType), mediaType)
                    }
                };
                var response = await GraphQlClient.SendQueryAsync<MediaSearchResponse>(request);
                var responseDynamic = await GraphQlClient.SendQueryAsync<dynamic>(request);
                var media = response.Data.Media;
                return null;
            }
            catch (GraphQLHttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                await Common.GrabarLogErrorAsync(ctx, ex.ToString());
                return null;
            }
            catch (Exception e)
            {
                await Common.GrabarLogErrorAsync(ctx, e.ToString());
                return null;
            }
        }

        public const string searchMediaQuery = @"
                query($search: String, $type: MediaType){
                    Media(search: $search, type: $type) {
                        id
                        title {
                            romaji
                            english
                        }
                        synonyms
                        description
                        siteUrl
                        coverImage {
                            large
                        }
                        format
                        chapters
                        episodes
                        status
                        meanScore
                        genres
                        seasonYear
                        startDate {
                            year
                            month
                            day
                        }
                        endDate {
                            year
                            month
                            day
                        }
                        tags {
                            name
                            isMediaSpoiler
                        }
                        studios {
                            nodes {
                                name
                                siteUrl
                            }
                        }
                        externalLinks {
                            site,
                            url
                        }
                        isAdult
                    }
                }
        ";
    }
}
