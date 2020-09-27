using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using Miki.GraphQL;
using Newtonsoft.Json;

namespace Discord_Bot.Modulos
{
    public class Games : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("quiz")]
        [Description("Adivina el personaje")]
        [RequireOwner] // temporal
        public async Task QuizCharactersGlobal(CommandContext ctx)
        {
            string query= "query($page: Int) {" +
                "Page(page: $page) {" +
                    "characters(sort: FAVOURITES_DESC){" +
                        "name{" +
                            "full" +
                        "}" +
                        "image{" +
                            "large" +
                        "}" +
                    "}" +
                "}" +
            "}";
           
            var graphQLClient = new GraphQLClient("https://graphql.anilist.co");

            var response = await graphQLClient.QueryAsync(query, new { page = 1 });

            //List<Character> characters = JsonConvert.DeserializeObject<List<Character>>(response);
            Page page = JsonConvert.DeserializeObject<Page>(response);
            //Data data = JsonConvert.DeserializeObject<Data>(response);
            //var data = JsonConvert.DeserializeObject<>(response);

            /*for (int i = 1; i <= 20; i++) // Top 1000
            {
                //var characters = await client.QueryAsync(query, new { page = i });
            }*/

            int asd = 1;
        }
    }
}
