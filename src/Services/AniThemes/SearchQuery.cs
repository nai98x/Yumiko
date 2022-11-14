namespace Yumiko.Services
{
    using DSharpPlus.SlashCommands;
    using Newtonsoft.Json;
    using RestSharp;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public static class SearchQuery
    {
        public static async Task<Video?> Search(InteractionContext ctx, double timeout, string search)
        {
            try
            {
                var client = new RestClient($"{Constants.AniThemesAPIUrl}/search?q={search}");
                var request = new RestRequest();

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful && response.Content != null)
                {
                    var data = JsonConvert.DeserializeObject<SearchResponse>(response.Content);
                    if (data == null || data.Search == null || data.Search.Videos == null || data.Search.Videos.Count == 0) 
                        return null;
                    var videos = data.Search.Videos.Take(25).ToList();

                    return await ChooseSongAsync(ctx, timeout, videos);
                    
                }

                return null;
            }
            catch (Exception e)
            {
                await Common.GrabarLogErrorAsync(ctx.Guild, ctx.Channel, $"Unknown error: {e.Message}\n{Formatter.BlockCode(e.StackTrace)}");
                return null;
            }
        }

        private static async Task<Video?> ChooseSongAsync(InteractionContext ctx, double timeout, List<Video> list)
        {
            list.Sort((x, y) => x.Filename.CompareTo(y.Filename));

            List<TitleDescription> opc = new();
            foreach (var item in list)
            {
                opc.Add(new TitleDescription
                {
                    Title = item.Filename
                });
            }

            var elegido = await Common.GetElegidoAsync(ctx, timeout, opc);
            if (elegido > 0) return list[elegido - 1];
            else return null;
        }
    }
}
