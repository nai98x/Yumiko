using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using YumikoBot.DAL;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace Discord_Bot.Modulos
{
    public class Otros : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("test"), Description("Testeos varios."), RequireOwner, Hidden]
        public async Task Test(CommandContext ctx)
        {
            // first retrieve the interactivity module from the client
            var interactivity = ctx.Client.GetInteractivity();

            // generate pages.
            var lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris vitae velit eget nunc iaculis laoreet vitae eu risus. Nullam sit amet cursus purus. Duis enim elit, malesuada consequat aliquam sit amet, interdum vel orci. Donec vehicula ut lacus consequat cursus. Aliquam pellentesque eleifend lectus vitae sollicitudin. Vestibulum sit amet risus rhoncus, hendrerit felis eget, tincidunt odio. Nulla sed urna ante. Mauris consectetur accumsan purus, ac dignissim ligula condimentum eu. Phasellus ullamcorper, arcu sed scelerisque tristique, ante elit tincidunt sapien, eu laoreet ipsum mauris eu justo. Curabitur mattis cursus urna, eu ornare lacus pulvinar in. Vivamus cursus gravida nunc. Sed dolor nisi, congue non hendrerit at, rutrum sed mi. Duis est metus, consectetur sed libero quis, dignissim gravida lacus. Mauris suscipit diam dolor, semper placerat justo sodales vel. Curabitur sed fringilla odio.\n\nMorbi pretium placerat nulla sit amet condimentum. Duis placerat, felis ornare vehicula auctor, augue odio consectetur eros, sit amet tristique dolor risus nec leo. Aenean vulputate ipsum sagittis augue malesuada, id viverra odio gravida. Curabitur aliquet elementum feugiat. Phasellus eu faucibus nibh, eget finibus nibh. Proin ac fermentum enim, non consequat orci. Nam quis elit vulputate, mollis eros ut, maximus lacus. Vivamus et lobortis odio. Suspendisse potenti. Fusce nec magna in eros tempor tincidunt non vel mi. Pellentesque auctor eros tellus, vel ultrices mi ultricies eu. Nam pharetra sed tortor id elementum. Donec sit amet mi eleifend, iaculis purus sit amet, interdum turpis.\n\nAliquam at consectetur lectus. Ut et ultrices augue. Etiam feugiat, tortor nec dictum pharetra, nulla mauris convallis magna, quis auctor libero ipsum vitae mi. Mauris posuere feugiat feugiat. Phasellus molestie purus sit amet ipsum sodales, eget pretium lorem pharetra. Quisque in porttitor quam, nec hendrerit ligula. Fusce tempus, diam ut malesuada semper, leo tortor vulputate erat, non porttitor nisi elit eget turpis. Nam vitae arcu felis. Aliquam molestie neque orci, vel consectetur velit mattis vel. Fusce eget tempus leo. Morbi sit amet bibendum mauris. Aliquam erat volutpat. Phasellus nunc lectus, vulputate vitae turpis vel, tristique vulputate nulla. Aenean sit amet augue at mauris laoreet convallis. Nam quis finibus dui, at lobortis lectus.\n\nSuspendisse potenti. Pellentesque massa enim, dapibus at tortor eu, posuere ultricies augue. Nunc condimentum enim id ex sagittis, ut dignissim neque tempor. Nulla cursus interdum turpis. Aenean auctor tempor justo, sed rhoncus lorem sollicitudin quis. Fusce non quam a ante suscipit laoreet eget at ligula. Aenean condimentum consectetur nunc, sit amet facilisis eros lacinia sit amet. Integer quis urna finibus, tristique justo ut, pretium lectus. Proin consectetur enim sed risus rutrum, eu vehicula augue pretium. Vivamus ultricies justo enim, id imperdiet lectus molestie at. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.\n\nNullam tincidunt dictum nibh, dignissim laoreet libero eleifend ut. Vestibulum eget maximus nulla. Suspendisse a auctor elit, ac facilisis tellus. Sed iaculis turpis ac purus tempor, ut pretium ante ultrices. Aenean commodo tempus vestibulum. Morbi vulputate pharetra molestie. Ut rhoncus quam felis, id mollis quam dapibus id. Curabitur faucibus id justo in ornare. Praesent facilisis dolor lorem, non vulputate velit finibus ut. Praesent vestibulum nunc ac nibh iaculis porttitor.\n\nFusce mattis leo sed ligula laoreet accumsan. Pellentesque tortor magna, ornare vitae tellus eget, mollis placerat est. Suspendisse potenti. Ut sit amet lacus sed nibh pulvinar mattis in bibendum dui. Mauris vitae turpis tempor, malesuada velit in, sodales lacus. Sed vehicula eros in magna condimentum vestibulum. Aenean semper finibus lectus, vel hendrerit lorem euismod a. Sed tempor ante quis magna sollicitudin, eu bibendum risus congue. Donec lectus sem, accumsan ut mollis et, accumsan sed lacus. Nam non dui non tellus pretium mattis. Mauris ultrices et felis ut imperdiet. Nam erat risus, consequat eu eros ac, convallis viverra sapien. Etiam maximus nunc et felis ultrices aliquam.\n\nUt tincidunt at magna at interdum. Sed fringilla in sem non lobortis. In dictum magna justo, nec lacinia eros porta at. Maecenas laoreet mattis vulputate. Sed efficitur tempor euismod. Integer volutpat a odio eu sagittis. Aliquam congue tristique nisi, quis aliquet nunc tristique vitae. Vivamus ac iaculis nunc, et faucibus diam. Donec vitae auctor ipsum, quis posuere est. Proin finibus, dolor ac euismod consequat, urna sem ultrices lectus, in iaculis sem nulla et odio. Integer et vulputate metus. Phasellus finibus et lorem eget lacinia. Maecenas velit est, luctus quis fermentum nec, fringilla eu lorem.\n\nPellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Mauris faucibus neque eu consectetur egestas. Mauris aliquet nibh pellentesque mollis facilisis. Duis egestas lectus sed justo sagittis ultrices. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Curabitur hendrerit quis arcu id dictum. Praesent in massa eget lectus pulvinar consectetur. Aliquam eget ipsum et velit congue porta vitae ut eros. Quisque convallis lacus et venenatis sagittis. Phasellus sit amet eros ac nibh facilisis laoreet vel eget nisi. In ante libero, volutpat in risus vel, tristique blandit leo. Morbi posuere bibendum libero, non efficitur mi sagittis vel. Cras viverra pulvinar pellentesque. Mauris auctor et lacus ut pellentesque. Nunc pretium luctus nisi eu convallis.\n\nSed nec ultricies arcu. Aliquam eu tincidunt diam, nec luctus ligula. Ut laoreet dignissim est, eu fermentum massa fermentum eget. Nullam non viverra justo, sed congue felis. Phasellus id convallis mauris. Aliquam elementum euismod ex, vitae dignissim nunc consectetur vitae. Donec ut odio quis ex placerat elementum sit amet eget lectus. Suspendisse potenti. Nam non massa id mi suscipit euismod. Nullam varius tincidunt diam congue congue. Proin pharetra vestibulum eros, vel imperdiet sem rutrum at. Cras eget gravida ligula, quis facilisis ex.\n\nEtiam consectetur elit mauris, euismod porta urna auctor a. Nulla facilisi. Praesent massa ipsum, iaculis non odio at, varius lobortis nisi. Aliquam viverra erat a dapibus porta. Pellentesque imperdiet maximus mattis. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec luctus elit sit amet feugiat convallis. Phasellus varius, sem ut volutpat vestibulum, magna arcu porttitor libero, in dapibus metus dolor nec dolor. Fusce at eleifend magna. Mauris cursus pellentesque sagittis. Nullam nec laoreet ante, in sodales arcu.";
            var lipsum_pages = interactivity.GeneratePagesInEmbed(lipsum);

            // send the paginator
            await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, lipsum_pages, timeoutoverride: TimeSpan.FromMinutes(5));

            await ctx.Channel.SendMessageAsync("uwu");
        }

        [Command("horarios"), Aliases("recordatorios", "horario", "recordatorio"), Description("Horarios para diversos paises.")]
        public async Task Horarios(CommandContext ctx, [RemainingText]string texto)
        {
            bool fechaPuesta = DateTime.TryParse(texto, CultureInfo.CreateSpecificCulture("es-ES"), DateTimeStyles.None, out DateTime timeUtc);
            bool ok = true;
            string error = string.Empty;
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
            string servers = string.Empty;
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
                    servers = string.Empty;
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
                    string resultados = string.Empty;
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
