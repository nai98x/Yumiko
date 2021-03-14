using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Otros : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("test"), Description("Testeos varios."), RequireOwner, Hidden]
        public async Task Test(CommandContext ctx)
        {
            await ctx.RespondAsync("uwu!");
        }

        [Command("reiniciar"), Aliases("restart"), Description("Se reinicia Yumiko."), RequireOwner]
        public async Task Reiniciar(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Ya vuelvo",
                Description = "Mi dueño está reiniciandome onii-chan",
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor()
            });
            await Task.Delay(3000);
            System.Diagnostics.Process.Start(AppDomain.CurrentDomain.FriendlyName);
            Environment.Exit(0);
        }

        [Command("apagar"), Description("Se apaga Yumiko."), RequireOwner]
        public async Task Stop(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
            var mensaje = await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = "Sayonara",
                Description = "Me voy onii-chan, mi dueño ha decidido apagarme " + DiscordEmoji.FromName(ctx.Client, ":sob:"),
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor()
            });
            await Task.Delay(3000);
            await mensaje.DeleteAsync("Auto borrado de Yumiko");
            Environment.Exit(0);
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

        [Command("descargar"), Description("Descarga los capitulos de un anime.")]
        public async Task DescargarAnime(CommandContext ctx, [RemainingText]string buscar)
        {
            AnimeFLVDownloader animeflv = new AnimeFLVDownloader();
            var interactivity = ctx.Client.GetInteractivity();
            var msgBusqueda = await ctx.RespondAsync(embed: new DiscordEmbedBuilder { 
                Title = "Buscando animes...",
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor()
            });
            var resBusqueda = await animeflv.Search(buscar);
            if(resBusqueda.Count > 0)
            {
                string resultados = "";
                int cont = 1;
                foreach(var res in resBusqueda)
                {
                    resultados += $"{cont} - **{res.name}** ({res.type})\n";
                    cont++;
                }
                await msgBusqueda.DeleteAsync("Auto borrado de Yumiko");
                var elegirRes = await ctx.RespondAsync(embed: new DiscordEmbedBuilder { 
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
                            var mensajeLinks = await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                                Title = "Descargar anime",
                                Description = $"Procesando links para **{elegido.name}**",
                                Footer = funciones.GetFooter(ctx),
                                Color = funciones.GetColor()
                            });
                            var links = await animeflv.GetLinks(elegido.href, elegido.name);
                            await mensajeLinks.DeleteAsync("Auto borrado de Yumiko");
                            Dictionary<string, Stream> dic = new Dictionary<string, Stream>
                            {
                                {"descargaLinks.txt",  (FileStream)funciones.CrearArchivo(links)}
                            };
                            await ctx.RespondAsync(new DiscordMessageBuilder { 
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
    }
}
