using DSharpPlus.CommandsNext;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System;
using System.Threading.Tasks;

namespace Discord_Bot
{
    public class FuncionesAnilist
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private readonly GraphQLHttpClient graphQLClient = new GraphQLHttpClient("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        public async Task<dynamic> GetAnilistMedia(CommandContext ctx, string busqueda, string tipo)
        {
            string query = "query($busqueda : String){" +
                "   Page(perPage:5){" +
                "       media(type: " + tipo.ToUpper() + ", search: $busqueda){" +
                "           title{" +
                "               romaji" +
                "           }," +
                "           coverImage{" +
                "               large" +
                "           }," +
                "           siteUrl," +
                "           description," +
                "           format," +
                "           chapters" +
                "           episodes" +
                "           status," +
                "           meanScore," +
                "           genres," +
                "           startDate{" +
                "               year," +
                "               month," +
                "               day" +
                "           }," +
                "           endDate{" +
                "               year," +
                "               month," +
                "               day" +
                "           }," +
                "           genres," +
                "           tags{" +
                "               name," +
                "               isMediaSpoiler" +
                "           }," +
                "           synonyms," +
                "           studios{" +
                "               nodes{" +
                "                   name," +
                "                   siteUrl" +
                "               }" +
                "           }," +
                "           externalLinks{" +
                "               site," +
                "               url" +
                "           }," +
                "           isAdult" +
                "       }" +
                "   }" +
                "}";

            var request = new GraphQLRequest
            {
                Query = query,
                Variables = new
                {
                    busqueda = busqueda
                }
            };
            try
            {
                var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                if (data.Data != null)
                {
                    //return data;
                }
                return data;
            }
            catch(Exception e)
            {
                await funciones.GrabarLogError(ctx, $"Error en query en FuncionesAnilist - GetAnilistMedia, utilizado: {tipo}\nError: {e.Message}");
            }
            return null;
        }
    }
}
