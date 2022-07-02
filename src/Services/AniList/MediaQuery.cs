namespace Yumiko.Services
{
    using DSharpPlus.SlashCommands;
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public static class MediaQuery
    {
        private static readonly GraphQLHttpClient GraphQlClient = new(Constants.AnilistAPIUrl, new NewtonsoftJsonSerializer());

        public static async Task<Media?> GetMedia(InteractionContext ctx, double timeout, string mediaSearch, MediaType mediaType)
        {
            try
            {
                var request = new GraphQLRequest
                {
                    Query = searchQuery,
                    Variables = new
                    {
                        search = mediaSearch,
                        type = Enum.GetName(typeof(MediaType), mediaType),
                        perPage = Constants.AnilistPerPage
                    }
                };
                var response = await GraphQlClient.SendQueryAsync<MediaPageResponse>(request);
                var results = response.Data.Page?.Media;

                return await ChooseMediaAsync(ctx, timeout, results!);
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

        public static async Task<Media?> GetMedia(InteractionContext ctx, int id, MediaType mediaType)
        {
            try
            {
                var request = new GraphQLRequest
                {
                    Query = idQuery,
                    Variables = new
                    {
                        id,
                        type = Enum.GetName(typeof(MediaType), mediaType),
                        perPage = Constants.AnilistPerPage
                    }
                };
                var response = await GraphQlClient.SendQueryAsync<MediaPageResponse>(request);
                return response!.Data.Page?.Media![0];
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

        private static async Task<Media?> ChooseMediaAsync(InteractionContext ctx, double timeout, List<Media> list)
        {
            List<TitleDescription> opc = new();
            foreach (var item in list)
            {
                string seasonYear = translations.not_yet_released;
                if (item.SeasonYear != null) seasonYear = item.SeasonYear.ToString()!;
                else if (item.StartDate.Year != null) seasonYear = $"{item.StartDate.Year}";

                opc.Add(new TitleDescription
                {
                    Title = item.Title.Romaji,
                    Description = $"{item.Format} - {seasonYear}"
                });
            }

            var elegido = await Common.GetElegidoAsync(ctx, timeout, opc);
            if (elegido > 0) return list[elegido - 1];
            else return null;
        }

        private const string searchQuery = @"
            query ($search: String, $type: MediaType, $perPage: Int){
                Page(perPage: $perPage) {
                    media(search: $search, type: $type) {
                        id
                        title {
                            romaji
                            english
                            native
                        }
                        synonyms
                        description
                        siteUrl
                        coverImage {
                            large
                            medium
                        }
                        bannerImage
                        format
                        volumes
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
                                isAnimationStudio
                            }
                        }
                        externalLinks {
                            site,
                            url
                        }
                        isAdult
                    }
                }
            }
        ";

        private const string idQuery = @"
            query ($id: Int, $type: MediaType, $perPage: Int){
                Page(perPage: $perPage) {
                    media(id: $id, type: $type) {
                        id
                        title {
                            romaji
                            english
                            native
                        }
                        synonyms
                        description
                        siteUrl
                        coverImage {
                            large
                            medium
                        }
                        bannerImage
                        format
                        volumes
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
                                isAnimationStudio
                            }
                        }
                        externalLinks {
                            site,
                            url
                        }
                        isAdult
                    }
                }
            }
        ";
    }
}
