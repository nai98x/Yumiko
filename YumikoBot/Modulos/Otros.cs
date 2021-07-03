using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
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
        private readonly FuncionesAuxiliares funciones = new();

        [Command("test"), Description("Testeos varios."), RequireOwner, Hidden]
        public async Task Test(CommandContext ctx)
        {
            DiscordMessageBuilder builder = new DiscordMessageBuilder()
                .WithContent("prueba")
                .AddComponents(new DiscordSelectComponent("dropdown", "Placeholder", new DiscordSelectComponentOption[]
                {
                    new DiscordSelectComponentOption("Label", "1", "Description", true, null),
                    new DiscordSelectComponentOption("Label2", "2", "Description2", false, null)
                },false,1 ,1));

            var msg = await ctx.RespondAsync(builder);

            var bt = await msg.WaitForSelectAsync("dropdown");

            if (!bt.TimedOut)
            {
                //var result = bt.Result;
                //int i = 0;
            }

            await ctx.Channel.SendMessageAsync("uwu!");
        }

        [Command("ping"), Description("Muestra el ping de Yumiko.")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Latencia",
                Description = "🏓 Pong! `" + ctx.Client.Ping.ToString() + " ms" + "`",
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor()
            }).ConfigureAwait(false);
        }

        [Command("invite"), Aliases("invitar"), Description("Muestra el link para invitar a Yumiko a un servidor.")]
        public async Task Invite(CommandContext ctx)
        {
            DiscordLinkButtonComponent button = new(ConfigurationManager.AppSettings["Invite"], "Invitación");
            DiscordMessageBuilder mensaje = new()
            {
                Content = "Puedes invitarme a un servidor haciendo click en el botón."
            };
            mensaje.AddComponents(button);
            await mensaje.SendAsync(ctx.Channel);
        }

        [Command("vote"), Aliases("votar"), Description("Muestra el link para votar a Yumiko.")]
        public async Task Vote(CommandContext ctx)
        {
            string url = "https://top.gg/bot/295182825521545218/vote";
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder()
            {
                Title = $"¡Votame en Top.gg!",
                Description = $"Puedes ayudarme votando en [este sitio web]({url}). ¡Gracias!",
                Color = funciones.GetColor(),
                Footer = funciones.GetFooter(ctx)
            }).ConfigureAwait(false);
        }

        [Command("info"), Description("Muestra la información de Yumiko.")]
        public async Task Info(CommandContext ctx)
        {
            var owner = ctx.Client.CurrentApplication.Owners.ElementAt(0);
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Información de Yumiko",
                Description = $"Bot de Discord creado por **{owner.Username}#{owner.Discriminator}**",
                Color = funciones.GetColor(),
                Footer = funciones.GetFooter(ctx)
            }.AddField("Version DSharpPlus", $"{ctx.Client.VersionString}", true)
            .AddField("Version API de Discord", $"V{ctx.Client.GatewayVersion}", true)
            .AddField("Version .NET Core", $"{Environment.Version}", true)
            ).ConfigureAwait(false);
        }

        [Command("contactar"), Aliases("sugerencia", "contact"), Description("Envia una sugerencia o peticion de contacto al desarrollador del bot.")]
        public async Task Contact(CommandContext ctx, [RemainingText]string texto)
        {
            if (String.IsNullOrEmpty(texto))
            {
                texto = await funciones.GetStringInteractivity(ctx, "Escribe tu mensaje", "Ejemplo: Hola! Encontre un bug en el juego de adivina el anime.", "Tiempo agotado esperando el texto", Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutGeneral"]));
            }
            if (!String.IsNullOrEmpty(texto))
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
        }

        [Command("servers"), Description("Muestra los servidores en los que esta Yumiko."), RequireOwner, Hidden]
        public async Task Servers(CommandContext ctx)
        {
            List<Page> pages = new();
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
                    pages.Add(new Page() { 
                        Embed = new DiscordEmbedBuilder
                        {
                            Title = "Servidores de Yumiko",
                            Description = servers,
                            Footer = new() { 
                                IconUrl = ctx.User.AvatarUrl,
                                Text = $"Página {pages.Count + 1} | Invocado por {ctx.Member.DisplayName} ({ctx.Member.Username}#{ctx.Member.Discriminator}) | {ctx.Prefix}{ctx.Command.Name}"
                            },
                            Color = funciones.GetColor()
                        }
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
            if (cont != 1)
            {
                pages.Add(new Page()
                {
                    Embed = new DiscordEmbedBuilder
                    {
                        Title = "Servidores de Yumiko",
                        Description = servers,
                        Footer = new()
                        {
                            IconUrl = ctx.User.AvatarUrl,
                            Text = $"Página {pages.Count + 1} | Invocado por {ctx.Member.DisplayName} ({ctx.Member.Username}#{ctx.Member.Discriminator}) | {ctx.Prefix}{ctx.Command.Name}"
                        },
                        Color = funciones.GetColor()
                    }
                });
            }

            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Servidores de Yumiko",
                Description = $"Cantidad: {ctx.Client.Guilds.Count}\nTotal de usuarios: {usuarios}",
            });
            _ = ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages).ConfigureAwait(false);
        }

        [Command("eliminarmensaje"), Description("Elimina un mensaje de Yumiko."), RequireOwner, Hidden]
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

        [Command("eliminarserver"), Aliases("eliminarservidor", "deleteguild"), Description("Elimina un server de Yumiko."), RequireOwner, Hidden]
        public async Task EliminarServidor(CommandContext ctx, ulong id)
        {
            var guild = await ctx.Client.GetGuildAsync(id);
            if (guild != null)
            {
                string nombre = guild.Name;
                await guild.LeaveAsync();
                await ctx.Channel.SendMessageAsync($"He salido del servidor **{nombre}**");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"No se encontró el servidor**");
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
                if (String.IsNullOrEmpty(buscar))
                {
                    buscar = await funciones.GetStringInteractivity(ctx, "Escriba el nombre del anime a descargar", "Ejemplo: Grisaia no Kajitsu", "Tiempo agotado esperando el nombre del anime", Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutGeneral"]));
                }
                if (!String.IsNullOrEmpty(buscar))
                {
                    MonoschinosDownloader animeflv = new();
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
                                    Dictionary<string, Stream> dic = new()
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

        [Command("serverso"), Description("Servers con el mismo owner."), RequireOwner, Hidden]
        public async Task ServersO(CommandContext ctx, int limite)
        {
            bool encontro = false;
            var guildsdesordenadas = ctx.Client.Guilds.Values;
            var lista = guildsdesordenadas.ToList();
            var guildsFiltrados = from com in lista
                                  group com by com.OwnerId;
            foreach(var guild in guildsFiltrados)
            {
                var cantidad = guild.Count();
                if(cantidad >= limite)
                {
                    encontro = true;
                    var owner = await ctx.Client.GetUserAsync(guild.Key);
                    string guilds = string.Empty;
                    foreach(var g in guild)
                    {
                        guilds +=
                            $"**{g.Name}**\n" +
                            $"  - **Id**: {g.Id}\n" +
                            $"  - **Fecha que entró Yumiko**: {g.JoinedAt}\n" +
                            $"  - **Miembros**: {g.MemberCount}\n\n";
                    }
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Author = new EmbedAuthor
                        {
                            Name = $"{owner.Username}#{owner.Discriminator}",
                            IconUrl = owner.AvatarUrl
                        },
                        Title = $"Owner con {cantidad} servidores",
                        Description = $"{guilds}",
                        Color = funciones.GetColor(),
                        Footer = funciones.GetFooter(ctx)
                    });
                }
            }
            if (!encontro)
            {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"Todo en orden!",
                    Description = $"No hay ningún owner con más de {limite} servidores",
                    Color = funciones.GetColor(),
                    Footer = funciones.GetFooter(ctx)
                });
            }
        }
    }
}
