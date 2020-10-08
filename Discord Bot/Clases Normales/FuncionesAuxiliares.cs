using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace Discord_Bot
{
    public class FuncionesAuxiliares
    {
        public int GetNumeroRandom(int min, int max)
        {
            var client = new RestClient("http://www.randomnumberapi.com/api/v1.0/random?min=" + min + "&max=" + max + "&count=1");
            var request = new RestRequest(Method.GET);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            IRestResponse response = client.Execute(request);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var resp = JsonConvert.DeserializeObject<dynamic>(response.Content);
                return resp.First;
            }
            return 0;
        }

        public string GetImagenRandomShip()
        {
            string[] opciones = new string[]
            {
                "https://i.imgur.com/nmXB1j3.gif",
                "https://i.imgur.com/apvPrPH.gif",
                "https://i.imgur.com/x3L3O3l.gif",
                "https://i.imgur.com/AgsLqLO.gif",
                "https://i.imgur.com/G4YLOal.gif",
                "https://i.imgur.com/gp3bj3R.gif",
                "https://i.imgur.com/EgmqF5t.gif",
                "https://i.imgur.com/aLSqypv.gif",
                "https://i.imgur.com/EI09P4S.gif"
            };
            Random rnd = new Random();
            return opciones[rnd.Next(opciones.Length -1)];
        }

        public EmbedFooter GetFooter(CommandContext ctx, string comando)
        {
            return  new EmbedFooter()
            {
                Text = $"Invocado por {ctx.Member.DisplayName} ({ctx.Member.Username}#{ctx.Member.Discriminator}) | {ConfigurationManager.AppSettings["Prefix"]}{comando}",
                IconUrl = ctx.Member.AvatarUrl
            };
        }
    }

    public class Imagen
    {
        public string Url { get; set; }
        public DiscordUser Autor { get; set; }
    }

    public class Character
    {
        public string NameFirst { get; set; }
        public string NameLast { get; set; }
        public string NameFull { get; set; }
        public string Image { get; set; }
        public string SiteUrl { get; set; }
    }

    public class Anime
    {
        public string TitleRomaji { get; set; }
        public string TitleEnglish { get; set; }
        public string Image { get; set; }
        public string SiteUrl { get; set; }
    }

    public class UsuarioJuego
    {
        public DiscordUser Usuario { get; set; }
        public int Puntaje { get; set; }
    }
}
