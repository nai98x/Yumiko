using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;
using DSharpPlus.VoiceNext;
using Newtonsoft.Json;
using System.Web;
using GoogleApi;

namespace Discord_Bot.Modulos
{
    public class Test : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        //[Command("test")]
        //public async Task Test1(CommandContext ctx)
        /*{
            string searchQuery = "aisaka";
            string cx = "001332085213878416933:9kql9y1jvyz";
            string apiKey = "AIzaSyDfuWQbvFmV8qWWXiZpl8aLEStTBIDp0o8";
            var request = WebRequest.Create("https://www.googleapis.com/customsearch/v1?key=" + apiKey + "&cx=" + cx + "&q=" + searchQuery);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseString = reader.ReadToEnd();
            dynamic jsonData = JsonConvert.DeserializeObject(responseString);

            var results = new List<Result>();
            foreach (var item in jsonData.items)
            {
                results.Add(new Result
                {
                    Title = item.title,
                    Link = item.link,
                    Snippet = item.snippet,
                });
            }

            await ctx.TriggerTypingAsync();
            foreach(var item in results)
            {
                await ctx.RespondAsync(item.Title + " " + item.Link );
            }*/
            /*
            GimageSearchClient client = new GimageSearchClient("http://www.google.com");
            IList<IImageResult> results;

            IAsyncResult result = client.BeginSearch(
               textSearch.Text.Trim(),  //param1
               int.Parse(textResult.Text), //param2
               ((arResult) => //param3
                {
                   results = client.EndSearch(arResult);
                   Dispatcher.Invoke(DispatcherPriority.Send,
                       (Action<IList<IImageResult>>)(async (data) =>
                       {
                           for (int i = 0; i < results.Count; i++)
                           {
                               Image img = new Image
                               {
                                   Source = await DownloadImage(results[i].TbImage.Url),
                                   Stretch = Stretch.UniformToFill,
                                   StretchDirection = StretchDirection.DownOnly,
                               };
                               wrapPanel.Children.Add(img);
                           }
                       }), null);
               }),
               null //param4
               )



        }*/

    }

    public class Result
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Snippet { get; set; }
        public string Source { get; set; }
        public string Query { get; set; }
        public int Index { get; set; }
    }
}
