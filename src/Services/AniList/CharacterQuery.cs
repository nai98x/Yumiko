namespace Yumiko.Services
{
    using DSharpPlus.SlashCommands;
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public static class CharacterQuery
    {
        private static readonly GraphQLHttpClient GraphQlClient = new(Constants.AnilistAPIUrl, new NewtonsoftJsonSerializer());

        public static async Task<Character?> GetCharacter(InteractionContext ctx, double timeout, string characterSearch)
        {
            try
            {
                var request = new GraphQLRequest
                {
                    Query = searchQuery,
                    Variables = new
                    {
                        search = characterSearch,
                        perPage = Constants.AnilistPerPage
                    }
                };
                var response = await GraphQlClient.SendQueryAsync<CharacterPageResponse>(request);
                var results = response.Data.Page.Characters;

                return await ChooseCharacterAsync(ctx, timeout, results);
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

        private static async Task<Character?> ChooseCharacterAsync(InteractionContext ctx, double timeout, List<Character> list)
        {
            List<TitleDescription> opc = new();
            foreach (var item in list)
            {
                string desc;
                if (item.Animes.Nodes.Count > 0)
                {
                    desc = item.Animes.Nodes[0].Title.Romaji;
                }
                else
                {
                    desc = "(Without animes)";
                }

                opc.Add(new TitleDescription
                {
                    Title = item.Name.Full,
                    Description = desc
                });
            }

            var elegido = await Common.GetElegidoAsync(ctx, timeout, opc);
            if (elegido > 0) return list[elegido - 1];
            else return null;
        }

        public const string searchQuery = @"
            query ($search: String, $perPage: Int){
                Page(perPage: $perPage) {
                    characters(search: $search) {
                        id
                        name {
                            full
                        }
                        image {
                            large
                        }
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
                        manga: media(type: MANGA) {
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
