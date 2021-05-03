using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using YumikoBot.DAL;

namespace Discord_Bot.Modulos
{
    public class Otros : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("test"), Description("Testeos varios."), RequireOwner, Hidden]
        public async Task Test(CommandContext ctx)
        {
            int n = 0;
            string resp = "";
            var timezones = TimeZoneInfo.GetSystemTimeZones();
            foreach(var t in timezones)
            {
                if(n < 50)
                {
                    resp += $"{t.Id}\n";
                    n++;
                }
                if(n >= 50)
                {
                    await ctx.RespondAsync(resp);
                    resp = "";
                    n = 0;
                }
            }
            if (n < 50)
            {
                await ctx.RespondAsync(resp);
            }
        }

        [Command("horarios"), Aliases("recordatorios", "horario", "recordatorio"), Description("Horarios para diversos paises.")]
        public async Task Horarios(CommandContext ctx, [RemainingText]string texto)
        {
            var interactivity = ctx.Client.GetInteractivity();
            bool ok = true;
            string error = "";
            var msgInicial = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Ingresa una fecha (En zona horaria UTC)",
                Description = "En este formato: **dd/mm/yyyy**\n  Ejemplo: 30/01/2000 23:15"
            });
            var msgFechaInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));
            if (!msgFechaInter.TimedOut)
            {
                bool result = funciones.ParsearFechaFromString(msgFechaInter.Result.Content, out DateTime timeUtc);
                if (result)
                {
                    try
                    {
                        TimeZoneInfo spainZone, argentinaZone, chileZone, venezuelaZone, uruguayZone, costaricaZone, panamaZone, mexicoZone;
                        DateTime spainTime, argentinaTime, chileTime, venezuelaTime, uruguayTime, costaricaTime, panamaTime, mexicoTime;

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            spainZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
                            spainTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, spainZone);

                            argentinaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
                            argentinaTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, argentinaZone);

                            chileZone = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");
                            chileTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, chileZone);

                            venezuelaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Caracas");
                            venezuelaTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, venezuelaZone);

                            uruguayZone = TimeZoneInfo.FindSystemTimeZoneById("America/Montevideo");
                            uruguayTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, uruguayZone);

                            costaricaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
                            costaricaTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, costaricaZone);

                            panamaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Panama");
                            panamaTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, panamaZone);

                            mexicoZone = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
                            mexicoTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, mexicoZone);

                            string desc =
                            $"**España:**     {spainTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {spainTime.Day} de {spainTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {spainTime.Year} a las {spainTime.ToString("HH:ss")}\n" +
                            $"**Argentina:**  {argentinaTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {argentinaTime.Day} de {argentinaTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {argentinaTime.Year} a las {argentinaTime.ToString("HH:ss")}\n" +
                            $"**Chile:**      {chileTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {chileTime.Day} de {chileTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {chileTime.Year} a las {chileTime.ToString("HH:ss")}\n" +
                            $"**Venezuela:**  {venezuelaTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {venezuelaTime.Day} de {venezuelaTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {venezuelaTime.Year} a las {venezuelaTime.ToString("HH:ss")}\n" +
                            $"**Uruguay:**    {uruguayTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {uruguayTime.Day} de {uruguayTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {uruguayTime.Year} a las {uruguayTime.ToString("HH:ss")}\n" +
                            $"**Costa Rica:** {costaricaTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {costaricaTime.Day} de {costaricaTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {costaricaTime.Year} a las {costaricaTime.ToString("HH:ss")}\n" +
                            $"**Mexico:**     {mexicoTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {mexicoTime.Day} de {mexicoTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {mexicoTime.Year} a las {mexicoTime.ToString("HH:ss")}\n" +
                            $"**Panama:**     {panamaTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {panamaTime.Day} de {panamaTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {panamaTime.Year} a las {panamaTime.ToString("HH:ss")}\n";

                            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                            {
                                Title = texto,
                                Description = desc,
                                Footer = funciones.GetFooter(ctx),
                                Color = funciones.GetColor()
                            });

                            if (msgInicial != null)
                                await msgInicial.DeleteAsync("Auto borrado de Yumiko");
                            if (msgFechaInter.Result != null)
                                await msgFechaInter.Result.DeleteAsync("Auto borrado de Yumiko");
                        }
                        else
                        {
                            ok = false;
                            error = "Comando no habilitado";
                        }
                    }
                    catch (TimeZoneNotFoundException)
                    {
                        ok = false;
                        error = "No se ha podido encontrar una zona horaria";
                    }
                    catch (InvalidTimeZoneException)
                    {
                        ok = false;
                        error = "Una zona horaria parece estar corrupta";
                    }
                }
                else
                {
                    ok = false;
                    error = "Fecha mal escrita";
                }
            }
            else
            {
                ok = false;
                error = "Tiempo agotado esperando la fecha";
            }

            if (!ok)
            {
                var msgError = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = error,
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor()
                });
                await Task.Delay(5000);
                if (msgError != null)
                    await msgError.DeleteAsync("Auto borrado de Yumiko");
                if (msgFechaInter.Result != null)
                    await msgFechaInter.Result.DeleteAsync("Auto borrado de Yumiko");
                if (msgInicial != null)
                    await msgInicial.DeleteAsync("Auto borrado de Yumiko");
            }
        }

        [Command("ping"), Description("Muestra el ping de Yumiko.")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Description = "🏓 Pong! `" + ctx.Client.Ping.ToString() + " ms" + "`",
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor()
            }).ConfigureAwait(false);
        }

        [Command("invite"), Aliases("invitar"), Description("Muestra el link para invitar a Yumiko a un servidor.")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.RespondAsync("Puedes invitarme a un servidor con este link:\n" + ConfigurationManager.AppSettings["Invite"]);
        }

        [Command("servers"), Description("Muestra los servidores en los que esta Yumiko."), RequireOwner]
        public async Task Servers(CommandContext ctx)
        {
            string servers = "";
            var guilds = ctx.Client.Guilds.Values;
            foreach (var guild in guilds)
            {
                servers += 
                    $"\n**{guild.Name}**\n" +
                    $"  - **Id**: {guild.Id}\n" +
                    $"  - **Fecha que entró Yumiko**: {guild.JoinedAt}\n" +
                    $"  - **Miembros**: {guild.MemberCount}\n";
            }
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = "Servidores de Yumiko",
                Description = servers,
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor()
            });
        }

        [Command("eliminarmensaje"), Description("Elimina un mensaje de Yumiko."), RequireOwner]
        public async Task BorrarMensajePropio(CommandContext ctx, ulong id)
        {
            var msg = await ctx.Channel.GetMessageAsync(id);
            if(msg != null && msg.Author.Id == ctx.Client.CurrentUser.Id)
            {
                await ctx.Channel.DeleteMessageAsync(msg, "Auto borrado de Yumiko");
            }
            else
            {
                var mensaje = await ctx.RespondAsync("Solo puedo borrar mis propios mensajes");
                await Task.Delay(3000);
                await mensaje.DeleteAsync("Auto borrado de Yumiko");
            }
        }

        [Command("descargar"), Description("Descarga los capitulos de un anime."), Hidden]
        public async Task DescargarAnime(CommandContext ctx, [RemainingText]string buscar)
        {
            if(
                ctx.Guild.Id == 748315008131268693 || // Eli y Nai
                ctx.Guild.Id == 701813281718927441 || // Anilist ESP
                ctx.Guild.Id == 713809173573271613 || // Yumiko
                ctx.Guild.Id == 741496210040553483 || // Maanhe
                ctx.Guild.Id == 520398815103025156 || // Tomy
                ctx.Guild.Id == 753023527623458916 // Andrea
                )
            {
                MonoschinosDownloader animeflv = new MonoschinosDownloader();
                var interactivity = ctx.Client.GetInteractivity();
                var msgBusqueda = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Buscando animes...",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor()
                });
                var resBusqueda = await animeflv.Search(buscar);
                if (resBusqueda.Count > 0)
                {
                    string resultados = "";
                    int cont = 1;
                    foreach (var res in resBusqueda)
                    {
                        resultados += $"{cont} - **{res.name}** ({res.type})\n";
                        cont++;
                    }
                    await msgBusqueda.DeleteAsync("Auto borrado de Yumiko");
                    var elegirRes = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Elije con un número el anime deseado",
                        Description = resultados,
                        Footer = funciones.GetFooter(ctx),
                        Color = funciones.GetColor()
                    });
                    var msgElegirInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));
                    if (!msgElegirInter.TimedOut)
                    {
                        bool result = int.TryParse(msgElegirInter.Result.Content, out int numElegir);
                        if (result)
                        {
                            if (numElegir > 0 && (numElegir <= resBusqueda.Count))
                            {
                                await elegirRes.DeleteAsync("Auto borrado de Yumiko");
                                await msgElegirInter.Result.DeleteAsync("Auto borrado de Yumiko");
                                var elegido = resBusqueda[numElegir - 1];
                                var mensajeLinks = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                                {
                                    Title = "Descargar anime",
                                    Description = $"Procesando links para **{elegido.name}**",
                                    Footer = funciones.GetFooter(ctx),
                                    Color = funciones.GetColor()
                                });
                                var links = await animeflv.GetLinks(elegido.href, elegido.name);
                                await mensajeLinks.DeleteAsync("Auto borrado de Yumiko");
                                Dictionary<string, Stream> dic = new Dictionary<string, Stream>
                            {
                                {"descargaLinks.txt",  (FileStream)funciones.CrearArchivoMonoschinos(links)}
                            };
                                await ctx.RespondAsync(new DiscordMessageBuilder
                                {
                                    Content = $"{ctx.User.Mention}, aquí tienes los links para descargar **{elegido.name}**",
                                }.WithFiles(dic));
                            }
                            else
                            {
                                var msg = await ctx.RespondAsync($"El número indicado debe ser valido");
                                await Task.Delay(3000);
                                await msg.DeleteAsync("Auto borrado de Yumiko");
                            }
                        }
                        else
                        {
                            var msg = await ctx.RespondAsync($"La eleccion debe ser indicada con un numero");
                            await Task.Delay(3000);
                            await msg.DeleteAsync("Auto borrado de Yumiko");
                        }
                    }
                    else
                    {
                        var msg = await ctx.RespondAsync($"Tiempo agotado esperando eleccion de anime");
                        await Task.Delay(3000);
                        await msg.DeleteAsync("Auto borrado de Yumiko");
                    }
                }
                else
                {
                    var msg = await ctx.RespondAsync($"No se encontraron resultados para {buscar}");
                    await Task.Delay(3000);
                    await msg.DeleteAsync("Auto borrado de Yumiko");
                    await msgBusqueda.DeleteAsync("Auto borrado de Yumiko");
                }
            }
            else
            {
                var msg = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Comando no habilitado",
                    Description = $"Hable con el owner del bot para habilitar este comando en el servidor",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor()
                });
                await Task.Delay(3000);
                await msg.DeleteAsync("Auto borrado de Yumiko");
            }
        }
    }
}
