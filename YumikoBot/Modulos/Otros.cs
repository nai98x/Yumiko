using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using YumikoBot.Data_Access_Layer;

namespace Discord_Bot.Modulos
{
    public class Otros : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("test"), RequireOwner, Hidden]
        public async Task Test(CommandContext ctx)
        {
            string texto = funciones.TraducirTexto("Hello world I'm Yumiko");
            await ctx.RespondAsync(texto);
        }

        [Command("reiniciar"), Aliases("restart"), RequireOwner, Description("Se reinicia Yumiko.")]
        public async Task Reiniciar(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
            await ctx.RespondAsync("Reiniciando..");
            System.Diagnostics.Process.Start(AppDomain.CurrentDomain.FriendlyName);
            Environment.Exit(0);
        }

        [Command("apagar"), RequireOwner, Description("Se apaga Yumiko.")]
        public async Task Stop(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
            var mensaje = await ctx.RespondAsync("Me voy onii-chan..");
            await Task.Delay(1000);
            await mensaje.DeleteAsync("Auto borrado de Yumiko");
            Environment.Exit(0);
        }

        [Command("ping"), Description("Muestra el ping de Yumiko.")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Color = funciones.GetColor(),
                Description = "🏓 Pong! `" + ctx.Client.Ping.ToString() + " ms" + "`"
            }).ConfigureAwait(false);
        }

        [Command("invite"), Aliases("invitar"), Description("Muestra el link para invitar a Yumiko a un servidor.")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.RespondAsync("Puedes invitarme a un servidor con este link:\n" + ConfigurationManager.AppSettings["Invite"]);
        }

        [Command("addReaccion"), Aliases("addReaction"), Description("Agrega una reaccion"), RequireOwner]
        public async Task AddReaccion(CommandContext ctx, [Description("Imagen de la reaccion")] string url)
        {
            var interactivity = ctx.Client.GetInteractivity();
            Imagenes imagenes = new Imagenes();
            var msgImagen = await ctx.RespondAsync("Indica el comando para la reaccion");
            var interact = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));
            if (!interact.TimedOut)
            {
                string comando = interact.Result.Content;
                imagenes.AddImagen(url, comando);
                var msg = await ctx.RespondAsync("Reaccion añadida");
                await Task.Delay(3000);
                await msg.DeleteAsync("Auto borrado de Yumiko");
                await interact.Result.DeleteAsync("Auto borrado de Yumiko");
                await msgImagen.DeleteAsync("Auto borrado de Yumiko");
            }
            else
            {
                var msg = await ctx.RespondAsync("Tiempo agotado esperando el comando");
                await Task.Delay(3000);
                await msg.DeleteAsync("Auto borrado de Yumiko");
                await msgImagen.DeleteAsync("Auto borrado de Yumiko");
            }
        }

        [Command("servers"), RequireOwner]
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

        [Command("eliminarmensaje"), RequireOwner]
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

        [Command("descargar")]
        public async Task DescargarAnime(CommandContext ctx, [RemainingText]string buscar)
        {
            AnimeFLVDownloader animeflv = new AnimeFLVDownloader();
            var interactivity = ctx.Client.GetInteractivity();
            var msgBusqueda = await ctx.RespondAsync("Buscando animes...");
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
                            var mensajeLinks = await ctx.RespondAsync("Procesando links...");
                            var elegido = resBusqueda[numElegir - 1];
                            var links = await animeflv.GetLinks(elegido.href, elegido.name);
                            await mensajeLinks.DeleteAsync("Auto borrado de Yumiko");
                            await ctx.RespondWithFileAsync(content:$"Aquí tienes los links para descargar **{elegido.name}** {ctx.User.Mention}" ,fileData: (FileStream)funciones.CrearArchivo(links));
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
            }
        }
    }
}
