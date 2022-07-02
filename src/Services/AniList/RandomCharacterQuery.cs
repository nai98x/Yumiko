namespace Yumiko.Services
{
    using DSharpPlus.SlashCommands;
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public static class RandomCharacterQuery
    {
        private static readonly GraphQLHttpClient GraphQlClient = new(Constants.AnilistAPIUrl, new NewtonsoftJsonSerializer());

        public static async Task<Character?> GetCharacter(InteractionContext ctx, int page)
        {
            try
            {
                var request = new GraphQLRequest
                {
                    Query = searchQuery,
                    Variables = new
                    {
                        page
                    }
                };
                var response = await GraphQlClient.SendQueryAsync<CharacterPageResponse>(request);
                var results = response.Data.Page?.Characters;

                return results![0];
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
            query ($page: Int){
                Page(perPage: 1, page: $page) {
                    characters(sort: FAVOURITES_DESC) {
                        id
                        name {
                            full
                        }
                        image {
                            large
                        }
                        favourites
                        siteUrl
                        description(asHtml: false)
                        animes: media(type: ANIME) {
                            nodes {
                                id
                                title {
                                    romaji
                                    english
                                    native
                                }
                                siteUrl
                            }
                        }
                        mangas: media(type: MANGA) {
                            nodes {
                                id
                                title {
                                    romaji
                                    english
                                    native
                                }
                                siteUrl
                            }
                        }
                    }
                }
            }
        ";
    }
}
