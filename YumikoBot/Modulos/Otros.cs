namespace Discord_Bot.Modulos
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using static DSharpPlus.Entities.DiscordEmbedBuilder;

    public class Otros : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("test"), Description("Testeos varios."), RequireOwner, Hidden]
        public async Task Test(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("uwu");
        }

        [Command("horarios"), Aliases("recordatorios", "horario", "recordatorio"), Description("Horarios para diversos paises.")]
        public async Task Horarios(CommandContext ctx, [RemainingText]string texto)
        {
            bool fechaPuesta = DateTime.TryParse(texto, CultureInfo.CreateSpecificCulture("es-ES"), DateTimeStyles.None, out DateTime timeUtc);
            bool ok = true;
            string error = "";
            if (!fechaPuesta)
            {
                var interactivity = ctx.Client.GetInteractivity();
                var msgInicial = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Ingresa una fecha (En zona horaria UTC)",
                    Description = "En este formato: **dd/mm/yyyy**\n  Ejemplo: 30/01/2000 23:15"
                });
                var msgFechaInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(60));
                if (!msgFechaInter.TimedOut)
                {
                    fechaPuesta = DateTime.TryParse(msgFechaInter.Result.Content, CultureInfo.CreateSpecificCulture("es-ES"), DateTimeStyles.None, out timeUtc);
                }
                if (msgFechaInter.Result != null)
                    await funciones.BorrarMensaje(ctx, msgFechaInter.Result.Id);
                if (msgInicial != null)
                    await funciones.BorrarMensaje(ctx, msgInicial.Id);
            }
            if (fechaPuesta)
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
                        $"**España:**     {spainTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {spainTime.Day} de {spainTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {spainTime.Year} a las {spainTime.ToString("HH:mm")}\n" +
                        $"**Argentina:**  {argentinaTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {argentinaTime.Day} de {argentinaTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {argentinaTime.Year} a las {argentinaTime.ToString("HH:mm")}\n" +
                        $"**Chile:**      {chileTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {chileTime.Day} de {chileTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {chileTime.Year} a las {chileTime.ToString("HH:mm")}\n" +
                        $"**Venezuela:**  {venezuelaTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {venezuelaTime.Day} de {venezuelaTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {venezuelaTime.Year} a las {venezuelaTime.ToString("HH:mm")}\n" +
                        $"**Uruguay:**    {uruguayTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {uruguayTime.Day} de {uruguayTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {uruguayTime.Year} a las {uruguayTime.ToString("HH:mm")}\n" +
                        $"**Costa Rica:** {costaricaTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {costaricaTime.Day} de {costaricaTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {costaricaTime.Year} a las {costaricaTime.ToString("HH:mm")}\n" +
                        $"**Mexico:**     {mexicoTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {mexicoTime.Day} de {mexicoTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {mexicoTime.Year} a las {mexicoTime.ToString("HH:mm")}\n" +
                        $"**Panama:**     {panamaTime.ToString("dddd", CultureInfo.CreateSpecificCulture("es"))} {panamaTime.Day} de {panamaTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"))} del {panamaTime.Year} a las {panamaTime.ToString("HH:mm")}\n";

                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "Horarios",
                            Description = desc,
                            Footer = funciones.GetFooter(ctx),
                            Color = funciones.GetColor()
                        });
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

            if (!ok)
            {
                var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = error,
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor()
                });
                await Task.Delay(5000);
                if (msgError != null)
                    await funciones.BorrarMensaje(ctx, msgError.Id);
            }
        }

        [Command("ping"), Description("Muestra el ping de Yumiko.")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Description = "🏓 Pong! `" + ctx.Client.Ping.ToString() + " ms" + "`",
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor()
            }).ConfigureAwait(false);
        }

        [Command("invite"), Aliases("invitar"), Description("Muestra el link para invitar a Yumiko a un servidor.")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Puedes invitarme a un servidor con este link:\n" + ConfigurationManager.AppSettings["Invite"]);
        }

        [Command("contactar"), Aliases("sugerencia", "contact"), Description("Envia una sugerencia o peticion de contacto al desarrollador del bot.")]
        public async Task Contact(CommandContext ctx, [RemainingText]string texto)
        {
            var LogGuild = await ctx.Client.GetGuildAsync(713809173573271613);
            var canalContacto = LogGuild.GetChannel(844384175925624852);
            await canalContacto.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Nuevo feedback",
                Footer = new EmbedFooter()
                {
                    Text = $"{ctx.User.Username}#{ctx.User.Discriminator} - {ctx.Message.Timestamp}",
                    IconUrl = ctx.User.AvatarUrl
                },
                Author = new EmbedAuthor()
                {
                    IconUrl = ctx.Guild.IconUrl,
                    Name = $"{ctx.Guild.Name}"
                },
                Color = DiscordColor.Green
            }.AddField("Id Servidor", $"{ctx.Guild.Id}", true)
            .AddField("Id Canal", $"{ctx.Channel.Id}", true)
            .AddField("Id Usuario", $"{ctx.User.Id}", true)
            .AddField("Canal", $"#{ctx.Channel.Name}", false)
            .AddField("Mensaje", $"{texto}", false));
        }

        [Command("servers"), Description("Muestra los servidores en los que esta Yumiko."), RequireOwner]
        public async Task Servers(CommandContext ctx)
        {
            var guildsdesordenadas = ctx.Client.Guilds.Values;
            var lista = guildsdesordenadas.ToList();
            lista.Sort((x, y) => x.JoinedAt.CompareTo(y.JoinedAt));
            string servers = "";
            int cont = 1;
            int usuarios = 0;
            int miembros;
            foreach (var guild in lista)
            {
                if (cont >= 10)
                {
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = servers,
                        Footer = funciones.GetFooter(ctx),
                        Color = funciones.GetColor()
                    });
                    cont = 1;
                    servers = "";
                }
                miembros = guild.MemberCount - 1;
                servers +=
                    $"**{guild.Name}**\n" +
                    $"  - **Id**: {guild.Id}\n" +
                    $"  - **Fecha que entró Yumiko**: {guild.JoinedAt}\n" +
                    $"  - **Miembros**: {guild.MemberCount}\n\n";
                usuarios += miembros;
                cont++;
            }
            if(cont != 1)
            {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Description = servers,
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor()
                });
            }
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Servidores de Yumiko",
                Description = $"Cantidad: {ctx.Client.Guilds.Count}\nTotal de usuarios: {usuarios}",
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
                var mensaje = await ctx.Channel.SendMessageAsync("Solo puedo borrar mis propios mensajes");
                await Task.Delay(3000);
                await funciones.BorrarMensaje(ctx, mensaje.Id);
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
                ctx.Guild.Id == 753023527623458916 || // Andrea
                ctx.Guild.Id == 724784732923232286 // Haze
                )
            {
                MonoschinosDownloader animeflv = new MonoschinosDownloader();
                var interactivity = ctx.Client.GetInteractivity();
                var msgBusqueda = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
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
                    await funciones.BorrarMensaje(ctx, msgBusqueda.Id);
                    var elegirRes = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
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
                                await funciones.BorrarMensaje(ctx, elegirRes.Id);
                                await funciones.BorrarMensaje(ctx, msgElegirInter.Result.Id);
                                var elegido = resBusqueda[numElegir - 1];
                                var mensajeLinks = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                {
                                    Title = "Descargar anime",
                                    Description = $"Procesando links para **{elegido.name}**",
                                    Footer = funciones.GetFooter(ctx),
                                    Color = funciones.GetColor()
                                });
                                var links = await animeflv.GetLinks(elegido.href, elegido.name);
                                await funciones.BorrarMensaje(ctx, mensajeLinks.Id);
                                Dictionary<string, Stream> dic = new Dictionary<string, Stream>
                                {
                                    {"descargaLinks.txt",  (FileStream)funciones.CrearArchivo(links)}
                                };
                                await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder
                                {
                                    Content = $"{ctx.User.Mention}, aquí tienes los links para descargar **{elegido.name}**",
                                }.WithFiles(dic));
                            }
                            else
                            {
                                var msg = await ctx.Channel.SendMessageAsync($"El número indicado debe ser valido");
                                await Task.Delay(5000);
                                await funciones.BorrarMensaje(ctx, msg.Id);
                                await funciones.BorrarMensaje(ctx, elegirRes.Id);
                                await funciones.BorrarMensaje(ctx, msgElegirInter.Result.Id);
                            }
                        }
                        else
                        {
                            var msg = await ctx.Channel.SendMessageAsync($"La eleccion debe ser indicada con un numero");
                            await Task.Delay(5000);
                            await funciones.BorrarMensaje(ctx, msg.Id);
                            await funciones.BorrarMensaje(ctx, elegirRes.Id);
                            await funciones.BorrarMensaje(ctx, msgElegirInter.Result.Id);
                        }
                    }
                    else
                    {
                        var msg = await ctx.Channel.SendMessageAsync($"Tiempo agotado esperando eleccion de anime");
                        await Task.Delay(5000);
                        await funciones.BorrarMensaje(ctx, msg.Id);
                        await funciones.BorrarMensaje(ctx, elegirRes.Id);
                        await funciones.BorrarMensaje(ctx, msgElegirInter.Result.Id);
                    }
                }
                else
                {
                    var msg = await ctx.Channel.SendMessageAsync($"No se encontraron resultados para {buscar}");
                    await Task.Delay(5000);
                    await funciones.BorrarMensaje(ctx, msg.Id);
                    await funciones.BorrarMensaje(ctx, msgBusqueda.Id);
                }
            }
            else
            {
                var msg = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Comando no habilitado",
                    Description = $"Hable con el owner del bot para habilitar este comando en el servidor",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor()
                });
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, msg.Id);   
            }
        }
    }
}
