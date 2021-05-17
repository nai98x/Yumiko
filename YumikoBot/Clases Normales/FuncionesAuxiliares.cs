using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using static DSharpPlus.Entities.DiscordEmbedBuilder;
using System.Linq;
using YumikoBot;
using System.Globalization;
using DiscordBotsList.Api;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus;

namespace Discord_Bot
{
    public class FuncionesAuxiliares
    {
        static Timer timer;

        public async Task<Imagen> GetImagenDiscordYumiko(CommandContext ctx, ulong idChannel)
        {
            DiscordGuild discordOOC = await ctx.Client.GetGuildAsync(713809173573271613);
            if (discordOOC == null)
            {
                await ctx.Channel.SendMessageAsync("Error al obtener servidor").ConfigureAwait(false);
                return null;
            }
            DiscordChannel channel = discordOOC.GetChannel(idChannel);
            if (channel == null)
            {
                await ctx.Channel.SendMessageAsync("Error al obtener canal del servidor").ConfigureAwait(false);
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

        public int GetNumeroRandom(int min, int max)
        {
            if (min <= 0 && max <= 0)
                return 0;
            Random rnd = new Random();
            return rnd.Next(minValue: min, maxValue: max);
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

        public Stream CrearArchivoAnimeFLV(AnimeLinks links)
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

        public Stream CrearArchivoMonoschinos(AnimeLinks links)
        {
            string path = $@"c:\temp\descargaLinks.txt";
            using (FileStream fs = File.Create(path))
            {
                string linksList = $"Links de descarga para {links.name}\n\n";
                var hosts = links.hosts;
                foreach (var host in hosts)
                {
                    linksList += $"Episodio: {host.name}\n";
                    var linkList = host.links;
                    foreach (var l in linkList)
                    {
                        linksList += $"{l.href}\n";
                    }
                    linksList += "\n";
                }
                byte[] info = new UTF8Encoding(true).GetBytes(linksList);
                fs.Write(info, 0, info.Length);
            }
            return File.OpenRead(path);
        }

        public bool ChequearPermisoYumiko(CommandContext ctx, DSharpPlus.Permissions permiso)
        {
            return DSharpPlus.PermissionMethods.HasPermission(ctx.Channel.PermissionsFor(ctx.Guild.CurrentMember), permiso);
        }

        public async Task BorrarMensaje(CommandContext ctx, ulong msgId)
        {
            if(ChequearPermisoYumiko(ctx, Permissions.ManageMessages))
            {
                try
                {
                    var mensaje = await ctx.Channel.GetMessageAsync(msgId);
                    if (mensaje != null)
                    {
                        await mensaje.DeleteAsync("Auto borrado de Yumiko");
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        public async Task ChequearVotoTopGG(CommandContext ctx)
        {
            IDebuggingService mode = new DebuggingService();
            bool debug = mode.RunningInDebugMode();
            if (!debug)
            {
                if (GetNumeroRandom(1, 10) == 1)
                {
                    /*
                    var json = string.Empty;
                    using (var fs = File.OpenRead("config.json"))
                    {
                        using var sr = new StreamReader(fs, new UTF8Encoding(false));
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    }
                    var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

                    AuthDiscordBotListApi DblApi = new AuthDiscordBotListApi(ctx.Client.CurrentUser.Id, configJson.TopGG_token);
                    bool voto = await DblApi.HasVoted(ctx.User.Id).ConfigureAwait(false);
                    */
                    bool voto = false;
                    if (!voto)
                    {
                        string url = "https://top.gg/bot/295182825521545218/vote";
                        var mensaje = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
                        {
                            Title = $"¡Votame en Top.gg!",
                            Description = $"Puedes ayudarme votando en [este sitio web]({url}). ¡Gracias!",
                            Color = GetColor()
                        }).ConfigureAwait(false);
                        await Task.Delay(10000);
                        await BorrarMensaje(ctx, mensaje.Id);
                    }
                }
            }
        }

        public async Task UpdateStatsTopGG(DiscordClient c)
        {
            var json = string.Empty;
            using (var fs = File.OpenRead("config.json"))
            {
                using var sr = new StreamReader(fs, new UTF8Encoding(false));
                json = await sr.ReadToEndAsync().ConfigureAwait(false);
            }
            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            AuthDiscordBotListApi DblApi = new AuthDiscordBotListApi(c.CurrentUser.Id, configJson.TopGG_token);
            var guilds = c.Guilds.Count;
            await DblApi.UpdateStats(guildCount: c.Guilds.Count);
        }

        public async Task<DateTime?> CrearDate(CommandContext ctx)
        {
            DiscordMessage msgDia, msgMes, msgAnio, error;
            DSharpPlus.Interactivity.InteractivityResult<DiscordMessage> msgDiaInter, msgMesInter, msgAnioInter;
            var interactivity = ctx.Client.GetInteractivity();
            msgDia = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Escribe el dia tu fecha de nacimiento",
                Description = "Ejemplo: 30"
            });
            msgDiaInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(60));
            if (!msgDiaInter.TimedOut)
            {
                bool resultDia = int.TryParse(msgDiaInter.Result.Content, out int dia);
                if (resultDia)
                {
                    msgMes = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Escribe el mes tu fecha de nacimiento",
                        Description = "Ejemplo: 1"
                    });
                    msgMesInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(60));
                    if (!msgMesInter.TimedOut)
                    {
                        bool resultMes = int.TryParse(msgMesInter.Result.Content, out int mes);
                        if (resultMes)
                        {
                            msgAnio = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                            {
                                Title = "Escribe el año tu fecha de nacimiento",
                                Description = "Ejemplo: 2000"
                            });
                            msgAnioInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(60));
                            if (!msgAnioInter.TimedOut)
                            {
                                bool resultAnio = int.TryParse(msgAnioInter.Result.Content, out int anio);
                                if (resultAnio)
                                {
                                    bool result = DateTime.TryParse($"{dia}/{mes}/{anio}", CultureInfo.CreateSpecificCulture("es-ES"), DateTimeStyles.None, out DateTime fecha);
                                    if (result)
                                    {
                                        if(fecha >= DateTime.Today)
                                        {
                                            error = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                            {
                                                Title = "Error",
                                                Description = "La fecha de cumpleaños no puede ser posterior a la actual",
                                                Footer = GetFooter(ctx),
                                                Color = GetColor()
                                            });
                                            await Task.Delay(5000);
                                            await BorrarMensaje(ctx, msgDia.Id);
                                            await BorrarMensaje(ctx, msgDiaInter.Result.Id);
                                            await BorrarMensaje(ctx, msgMes.Id);
                                            await BorrarMensaje(ctx, msgMesInter.Result.Id);
                                            await BorrarMensaje(ctx, msgAnio.Id);
                                            await BorrarMensaje(ctx, msgAnioInter.Result.Id);
                                            await BorrarMensaje(ctx, error.Id);
                                        }
                                        else
                                        {
                                            await BorrarMensaje(ctx, msgDia.Id);
                                            await BorrarMensaje(ctx, msgDiaInter.Result.Id);
                                            await BorrarMensaje(ctx, msgMes.Id);
                                            await BorrarMensaje(ctx, msgMesInter.Result.Id);
                                            await BorrarMensaje(ctx, msgAnio.Id);
                                            await BorrarMensaje(ctx, msgAnioInter.Result.Id);
                                            return fecha;
                                        }
                                    }
                                    else
                                    {
                                        error = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                        {
                                            Title = "Error",
                                            Description = $"La fecha `{dia}/{mes}/{anio}` no es real",
                                            Footer = GetFooter(ctx),
                                            Color = GetColor()
                                        });
                                        await Task.Delay(5000);
                                        await BorrarMensaje(ctx, msgDia.Id);
                                        await BorrarMensaje(ctx, msgDiaInter.Result.Id);
                                        await BorrarMensaje(ctx, msgMes.Id);
                                        await BorrarMensaje(ctx, msgMesInter.Result.Id);
                                        await BorrarMensaje(ctx, msgAnio.Id);
                                        await BorrarMensaje(ctx, msgAnioInter.Result.Id);
                                        await BorrarMensaje(ctx, error.Id);
                                    }
                                }
                                else
                                {
                                    error = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                    {
                                        Title = "Error",
                                        Description = "El año debe ser un numero",
                                        Footer = GetFooter(ctx),
                                        Color = GetColor()
                                    });
                                    await Task.Delay(5000);
                                    await BorrarMensaje(ctx, msgDia.Id);
                                    await BorrarMensaje(ctx, msgDiaInter.Result.Id);
                                    await BorrarMensaje(ctx, msgMes.Id);
                                    await BorrarMensaje(ctx, msgMesInter.Result.Id);
                                    await BorrarMensaje(ctx, msgAnio.Id);
                                    await BorrarMensaje(ctx, msgAnioInter.Result.Id);
                                    await BorrarMensaje(ctx, error.Id);
                                }
                            }
                            else
                            {
                                error = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                {
                                    Title = "Error",
                                    Description = "Tiempo agotado esperando el año",
                                    Footer = GetFooter(ctx),
                                    Color = GetColor()
                                });
                                await Task.Delay(5000);
                                await BorrarMensaje(ctx, msgDia.Id);
                                await BorrarMensaje(ctx, msgDiaInter.Result.Id);
                                await BorrarMensaje(ctx, msgMes.Id);
                                await BorrarMensaje(ctx, msgMesInter.Result.Id);
                                await BorrarMensaje(ctx, msgAnio.Id);
                                await BorrarMensaje(ctx, error.Id);
                            }
                        }
                        else
                        {
                            error = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                            {
                                Title = "Error",
                                Description = "El mes debe ser un numero",
                                Footer = GetFooter(ctx),
                                Color = GetColor()
                            });
                            await Task.Delay(5000);
                            await BorrarMensaje(ctx, msgDia.Id);
                            await BorrarMensaje(ctx, msgDiaInter.Result.Id);
                            await BorrarMensaje(ctx, msgMes.Id);
                            await BorrarMensaje(ctx, msgMesInter.Result.Id);
                            await BorrarMensaje(ctx, error.Id);
                        }
                    }
                    else
                    {
                        error = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "Error",
                            Description = "Tiempo agotado esperando el mes",
                            Footer = GetFooter(ctx),
                            Color = GetColor()
                        });
                        await Task.Delay(5000);
                        await BorrarMensaje(ctx, msgDia.Id);
                        await BorrarMensaje(ctx, msgDiaInter.Result.Id);
                        await BorrarMensaje(ctx, msgMes.Id);
                        await BorrarMensaje(ctx, error.Id);
                    }
                }
                else
                {
                    error = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Error",
                        Description = "El dia debe ser un numero",
                        Footer = GetFooter(ctx),
                        Color = GetColor()
                    });
                    await Task.Delay(5000);
                    await BorrarMensaje(ctx, msgDia.Id);
                    await BorrarMensaje(ctx, msgDiaInter.Result.Id);
                    await BorrarMensaje(ctx, error.Id);
                }
            }
            else
            {
                error = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "Tiempo agotado esperando el dia",
                    Footer = GetFooter(ctx),
                    Color = GetColor()
                });
                await Task.Delay(5000);
                await BorrarMensaje(ctx, msgDia.Id);
                await BorrarMensaje(ctx, error.Id);
            }
            return null;
        }
    }
}
