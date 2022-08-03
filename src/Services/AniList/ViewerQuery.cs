namespace Yumiko.Services
{
    using DSharpPlus.SlashCommands;
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public static class ViewerQuery
    {
        public static async Task<User?> GetProfile(InteractionContext ctx, string token)
        {
            try
            {
                GraphQLHttpClient GraphQlClient = new(Constants.AnilistAPIUrl, new NewtonsoftJsonSerializer());
                if (GraphQlClient.HttpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    GraphQlClient.HttpClient.DefaultRequestHeaders.Remove("Authorization");
                }

                GraphQlClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var request = new GraphQLRequest
                {
                    Query = searchQuery
                };
                var response = await GraphQlClient.SendQueryAsync<ViewerResponse>(request);
                var result = response.Data.Viewer;

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
            query {
                Viewer {
                    id
                    name
                    siteUrl
                    avatar {
                        medium
                    }
                    bannerImage
                }
            }
        ";
    }
}
