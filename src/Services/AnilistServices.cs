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
    }
}
