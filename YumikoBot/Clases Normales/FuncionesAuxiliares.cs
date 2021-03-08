using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using YumikoBot.Data_Access_Layer;
using static DSharpPlus.Entities.DiscordEmbedBuilder;
using Google.Cloud.Translation.V2;
using System.Linq;

namespace Discord_Bot
{
    public class FuncionesAuxiliares
    {
        private readonly LeaderboardGeneral leaderboard = new LeaderboardGeneral();
        static Timer timer;

        public async Task<Imagen> GetImagenDiscordYumiko(CommandContext ctx, ulong idChannel)
        {
            List<Imagen> lista = new List<Imagen>();
            DiscordGuild discordOOC = await ctx.Client.GetGuildAsync(713809173573271613);
            if (discordOOC == null)
            {
                await ctx.RespondAsync("Error al obtener servidor").ConfigureAwait(false);
                return null;
            }
            DiscordChannel channel = discordOOC.GetChannel(idChannel);
            if (channel == null)
            {
                await ctx.RespondAsync("Error al obtener canal del servidor").ConfigureAwait(false);
                return null;
            }
            IReadOnlyList<DiscordMessage> mensajes = await channel.GetMessagesAsync();
            List<DiscordMessage> msgs = mensajes.ToList();
            int cntMensajes = msgs.Count();
            DiscordMessage last = msgs.LastOrDefault();
            while (cntMensajes == 100)
            {
                var mensajesAux = await channel.GetMessagesBeforeAsync(last.Id);

                cntMensajes = mensajesAux.Count();
                last = mensajesAux.LastOrDefault();

                foreach (DiscordMessage mensaje in mensajesAux)
                {
                    msgs.Add(mensaje);
                }
            }
            List<Imagen> opciones = new List<Imagen>();
            foreach (DiscordMessage msg in msgs)
            {
                var att = msg.Attachments.FirstOrDefault();
                if (att != null && att.Url != null)
                {
                    opciones.Add(new Imagen
                    {
                        Url = att.Url,
                        Autor = msg.Author
                    });
                }
            }
            int rnd = GetNumeroRandom(0, opciones.Count - 1);
            return opciones[rnd];
        }

        public string TraducirTexto(string texto)
        {
            var client = TranslationClient.Create();
            var response = client.TranslateText(texto, LanguageCodes.Spanish, LanguageCodes.English);
            return response.TranslatedText;
        }

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

        public string NormalizarField(string s)
        {
            if (s.Length > 1024)
            {
                string aux = s.Remove(1024);
                int index = aux.LastIndexOf('[');
                if (index != -1)
                    return aux.Remove(aux.LastIndexOf('[')) + "...";
                else
                    return aux.Remove(aux.Length - 4) + " ...";
            }
            return s;
        }

        public string NormalizarDescription(string s)
        {
            if (s.Length > 2048)
            {
                string aux = s.Remove(2048);
                int index = aux.LastIndexOf('[');
                if(index != -1)
                    return aux.Remove(aux.LastIndexOf('[')) + "...";
                else
                    return aux.Remove(aux.Length-4) + " ...";
            }
            return s;
        }

        public string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
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

        public EmbedFooter GetFooter(CommandContext ctx)
        {
            return  new EmbedFooter()
            {
                Text = $"Invocado por {ctx.Member.DisplayName} ({ctx.Member.Username}#{ctx.Member.Discriminator}) | {ctx.Prefix}{ctx.Command.Name}",
                IconUrl = ctx.Member.AvatarUrl
            };
        }

        public EmbedAuthor GetAuthor(string nombre, string avatar, string url)
        {
            return new EmbedAuthor()
            {
                IconUrl = avatar,
                Name = nombre,
                Url = url
            };
        }

        public DiscordColor GetColor()
        {
            return new DiscordColor(78, 63, 96);
        }

        public void ScheduleAction(DiscordChannel canal, DiscordMember miembro, DateTime scheduledTime)
        {
            DateTime nowTime = DateTime.Now;
            if (nowTime > scheduledTime)
                return;
            double tickTime = (double)(scheduledTime - DateTime.Now).TotalMilliseconds;
            timer = new Timer(tickTime);
            timer.Elapsed += async (sender, e) => await Timer_Elapsed(e, canal, miembro);
            timer.Start();
        }

        static async Task Timer_Elapsed(ElapsedEventArgs e, DiscordChannel canal, DiscordMember miembro)
        {
            await canal.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = $"Feliz cumpleaños {miembro.DisplayName}!",
                Description = $"Todos denle un gran saludo a {miembro.Mention}",
                ImageUrl = "https://data.whicdn.com/images/299405277/original.gif"
            });
            timer.Stop();
        }

        public string QuitarCaracteresEspeciales(string str)
        {
            if(str != null)
                return Regex.Replace(str, @"[^a-zA-Z0-9]+", " ").Trim();
            return null;
        }

        public string LimpiarTexto(string texto)
        {
            if (texto != null)
            {
                texto = texto.Replace("<br>", "");
                texto = texto.Replace("<Br>", "");
                texto = texto.Replace("<bR>", "");
                texto = texto.Replace("<BR>", "");
                texto = texto.Replace("<i>", "*");
                texto = texto.Replace("<I>", "*");
                texto = texto.Replace("</i>", "*");
                texto = texto.Replace("</I>", "*");
                texto = texto.Replace("~!", "||");
                texto = texto.Replace("!~", "||");
                texto = texto.Replace("__", "**");
            }
            else
            {
                texto = "";
            }
            return texto;
        }

        public Stream CrearArchivo(AnimeLinks links)
        {
            string path = $@"c:\temp\descargaLinks.txt";
            using (FileStream fs = File.Create(path))
            {
                string linksList = $"Links de descarga para {links.name}\n\n";
                var hosts = links.hosts;
                foreach(var host in hosts)
                {
                    linksList += $"Servidor: {host.name}\n";
                    var linkList = host.links;
                    foreach(var l in linkList)
                    {
                        linksList += $"{l.number} - {l.href}\n";
                    }
                    linksList += "\n";
                }
                byte[] info = new UTF8Encoding(true).GetBytes(linksList);
                fs.Write(info, 0, info.Length);
            }
            return File.OpenRead(path);
        }
    }
}
