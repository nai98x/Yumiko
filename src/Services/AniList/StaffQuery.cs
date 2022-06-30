namespace Yumiko.Services
{
    using DSharpPlus.SlashCommands;
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public static class StaffQuery
    {
        private static readonly GraphQLHttpClient GraphQlClient = new(Constants.AnilistAPIUrl, new NewtonsoftJsonSerializer());

        public static async Task<Staff?> GetStaff(InteractionContext ctx, double timeout, string staffSearch)
        {
            try
            {
                var request = new GraphQLRequest
                {
                    Query = searchQuery,
                    Variables = new
                    {
                        search = staffSearch,
                        perPage = Constants.AnilistPerPage
                    }
                };
                var response = await GraphQlClient.SendQueryAsync<StaffPageResponse>(request);
                var results = response.Data.Page!.Staffs!;

                if(results.Count == 0) return null;
                return await ChooseStaffAsync(ctx, timeout, results!);
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

        private static async Task<Staff?> ChooseStaffAsync(InteractionContext ctx, double timeout, List<Staff> list)
        {
            List<TitleDescription> opc = new();
            foreach (var item in list)
            {
                opc.Add(new TitleDescription
                {
                    Title = item.Name.Full
                });
            }

            var elegido = await Common.GetElegidoAsync(ctx, timeout, opc);
            if (elegido > 0) return list[elegido - 1];
            else return null;
        }

        private const string searchQuery = @"
            query ($search: String, $perPage: Int){
                Page(perPage: $perPage) {
                    staff(search: $search) {
                        id
                        name {
                            full
                        }
                        image {
                            large
                        }
                        languageV2
                        description(asHtml: false)
                        siteUrl
                        gender
                        age
                        dateOfBirth {
                            year
                            month
                            day
                        }
                        dateOfDeath {
                            year
                            month
                            day
                        }
                    }
                }
            }
        ";
    }
}
