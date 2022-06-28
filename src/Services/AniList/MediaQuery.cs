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

        //public static async Task<DiscordEmbedBuilder?> GetInfoMediaUser(InteractionContext ctx, int anilistId, int mediaId)
        //public static async Task<DiscordEmbedBuilder?> GetUserRecommendationsAsync(InteractionContext ctx, DiscordUser user, MediaType type, int anilistUserId)

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
                var results = response.Data.Page.Media;

                return await ChooseMediaAsync(ctx, timeout, results);
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
            List<AnimeShort> opc = new();
            foreach (var item in list)
            {
                string seasonYear = translations.not_yet_released;
                if (item.seasonYear != null) seasonYear = item.seasonYear.ToString()!;

                opc.Add(new AnimeShort
                {
                    Title = item.title.romaji,
                    Description = $"{item.format} - {seasonYear}"
                });
            }

            var elegido = await Common.GetElegidoAsync(ctx, timeout, opc);
            if (elegido > 0) return list[elegido - 1];
            else return null;
        }

        public const string searchQuery = @"
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
