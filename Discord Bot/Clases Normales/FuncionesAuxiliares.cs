using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
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
                Text = $"Invocado por {ctx.Member.DisplayName} ({ctx.Member.Username}#{ctx.Member.Discriminator}) | {ConfigurationManager.AppSettings["Prefix"]}{ctx.Command.Name}",
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

        public string LimpiarTexto(string texto)
        {
            if (texto != null)
            {
                texto = texto.Replace("<br>", "\n");
                texto = texto.Replace("<Br>", "\n");
                texto = texto.Replace("<bR>", "\n");
                texto = texto.Replace("<BR>", "\n");
                texto = texto.Replace("<i>", "*");
                texto = texto.Replace("<I>", "*");
                texto = texto.Replace("</i>", "*");
                texto = texto.Replace("</I>", "*");
                texto = texto.Replace("~!", "||");
                texto = texto.Replace("!~", "||");
            }
            else
            {
                texto = "";
            }
            return texto;
        }

        public async Task GetResultados(CommandContext ctx, List<UsuarioJuego> participantes, int rondas)
        {
            string resultados = "";
            participantes.Sort((x, y) => y.Puntaje.CompareTo(x.Puntaje));
            int tot = 0;
            foreach (UsuarioJuego uj in participantes)
            {
                resultados += $"- {uj.Usuario.Username}#{uj.Usuario.Discriminator}: {uj.Puntaje} aciertos\n";
                tot += uj.Puntaje;
            }
            resultados += $"\nTotal ({tot}/{rondas})";
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
            {
                Title = "Resultados",
                Description = resultados,
                Color = GetColor()
            }).ConfigureAwait(false);
        }

        public async Task<SettingsJuego> InicializarJuego(CommandContext ctx, InteractivityExtension interactivity)
        {
            var msgCntRondas = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Elige la cantidad de rondas (máximo 100)",
                Description = "Por ejemplo: 10"
            });
            var msgRondasInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
            if (!msgRondasInter.TimedOut)
            {
                bool result = int.TryParse(msgRondasInter.Result.Content, out int rondas);
                if (result)
                {
                    if (rondas > 0 && rondas <= 100)
                    {
                        var msgDificultad = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "Elije la dificultad",
                            Description = "1- Fácil\n2- Media\n3- Dificil\n4- Extremo\n 5- Kusan"
                        });
                        var msgDificultadInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
                        if (!msgDificultadInter.TimedOut)
                        {
                            result = int.TryParse(msgDificultadInter.Result.Content, out int dificultad);
                            if (result)
                            {
                                string dificultadStr;
                                if (dificultad == 1 || dificultad == 2 || dificultad == 3 || dificultad == 4 || dificultad == 5)
                                {
                                    int iterIni;
                                    int iterFin;
                                    string orden;
                                    switch (dificultad)
                                    {
                                        case 1:
                                            iterIni = 1;
                                            iterFin = 6;
                                            dificultadStr = "Fácil";
                                            orden = "FAVOURITES_DESC";
                                            break;
                                        case 2:
                                            iterIni = 6;
                                            iterFin = 20;
                                            dificultadStr = "Media";
                                            orden = "FAVOURITES_DESC";
                                            break;
                                        case 3:
                                            iterIni = 20;
                                            iterFin = 60;
                                            dificultadStr = "Dificil";
                                            orden = "FAVOURITES_DESC";
                                            break;
                                        case 4:
                                            iterIni = 60;
                                            iterFin = 100;
                                            dificultadStr = "Extremo";
                                            orden = "FAVOURITES_DESC";
                                            break;
                                        case 5:
                                            iterIni = 1;
                                            iterFin = 40;
                                            dificultadStr = "Kusan";
                                            orden = "FAVOURITES";
                                            break;
                                        default:
                                            iterIni = 6;
                                            iterFin = 20;
                                            dificultadStr = "Medio";
                                            orden = "FAVOURITES_DESC";
                                            break;
                                    }
                                    await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
                                    await msgCntRondas.DeleteAsync("Auto borrado de Yumiko");
                                    await msgRondasInter.Result.DeleteAsync("Auto borrado de Yumiko");
                                    await msgDificultad.DeleteAsync("Auto borrado de Yumiko");
                                    await msgDificultadInter.Result.DeleteAsync("Auto borrado de Yumiko");
                                    return new SettingsJuego()
                                    {
                                        Ok = true,
                                        Rondas = rondas,
                                        IterIni = iterIni,
                                        IterFin = iterFin,
                                        Dificultad = dificultadStr,
                                        Orden = orden
                                    };
                                }
                                else
                                {
                                    return new SettingsJuego()
                                    {
                                        Ok = false,
                                        MsgError = "La dificultad debe ser 1, 2, 3 o 4"
                                    };
                                }
                            }
                            else
                            {
                                return new SettingsJuego()
                                {
                                    Ok = false,
                                    MsgError = "La dificultad debe ser un número (1, 2, 3 o 4)"
                                };
                            }
                        }
                        else
                        {
                            return new SettingsJuego()
                            {
                                Ok = false,
                                MsgError = "Tiempo agotado esperando la dificultad"
                            };
                        }
                    }
                    else
                    {
                        return new SettingsJuego()
                        {
                            Ok = false,
                            MsgError = "La cantidad de rondas debe ser mayor a 0 y menor a 100"
                        };
                    }
                }
                else
                {
                    return new SettingsJuego()
                    {
                        Ok = false,
                        MsgError = "La cantidad de rondas debe ser un numero"
                    };
                }
            }
            else
            {
                return new SettingsJuego()
                {
                    Ok = false,
                    MsgError = "Tiempo agotado esperando la cantidad de rondas"
                };
            }
        }
    }
}
