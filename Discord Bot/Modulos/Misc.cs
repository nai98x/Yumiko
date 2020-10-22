using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using RestSharp;
using System.Net;
using Newtonsoft.Json;

namespace Discord_Bot.Modulos
{
    public class Misc : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Color = new DiscordColor(78, 63, 96),
                Description = "🏓 Pong! `" + ctx.Client.Ping.ToString() + " ms" + "`"
            }).ConfigureAwait(false);
        }

        [Command("say"), Aliases("s")]
        public async Task Say(CommandContext ctx, [RemainingText]string mensaje)
        {
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(mensaje).ConfigureAwait(false);
        }

        [Command("avatar")]
        public async Task Avatar(CommandContext ctx, DiscordUser usuario = null)
        {
            if(usuario == null)
            {
                usuario = ctx.User;
            }
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder { 
                Title = $"Avatar de {usuario.Username}#{usuario.Discriminator}",
                ImageUrl = usuario.AvatarUrl,
                Footer = funciones.GetFooter(ctx, "avatar"),
                Color = new DiscordColor(78, 63, 96)
            }).ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }

        [Command("pregunta"), Aliases("p", "question", "sisonon")]
        public async Task Sisonon(CommandContext ctx, [RemainingText]string pregunta)
        {
            Random rnd = new Random();
            int random = rnd.Next(2);
            switch (random)
            {
                case 0:
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = funciones.GetFooter(ctx, "pregunta"),
                        Color = DiscordColor.Red,
                        Title = "¿SIS O NON?",
                        Description = "**Pregunta:** " + pregunta + "\n**Respuesta:** NON"
                    }).ConfigureAwait(false);
                    break;
                case 1:
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = funciones.GetFooter(ctx, "pregunta"),
                        Color = DiscordColor.Green,
                        Title = "¿SIS O NON?",
                        Description = "**Pregunta:** " + pregunta + "\n**Respuesta:** SIS"
                    }).ConfigureAwait(false);
                    break;
            }
            await ctx.Message.DeleteAsync();
        }

        [Command("elegir"), Aliases("e")]
        public async Task Elegir(CommandContext ctx, [RemainingText]string pregunta)
        {
            var interactivity = ctx.Client.GetInteractivity();
            DiscordMessage mensajeBot = await ctx.Channel.SendMessageAsync("Ingrese las opciones separadas por comas").ConfigureAwait(false);
            var msg = await interactivity.WaitForMessageAsync(xm => xm.Author == ctx.User, TimeSpan.FromSeconds(60));
            if (!msg.TimedOut)
            {
                List<string> opciones = new List<string>();
                string msgResponse = msg.Result.Content;
                opciones = msgResponse.Split(",").ToList();
                Random rnd = new Random();
                int random = rnd.Next(opciones.Count);
                string options = "**Opciones:**";
                foreach (string msj in opciones)
                {
                    options += "\n   - " + msj;
                }
                await ctx.Message.DeleteAsync().ConfigureAwait(false);
                await mensajeBot.DeleteAsync().ConfigureAwait(false);
                await msg.Result.DeleteAsync().ConfigureAwait(false);

                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx, "elegir"),
                    Color = new DiscordColor(78, 63, 96),
                    Title = "Pregunta",
                    Description = "** " + pregunta + "**\n\n" + options + "\n\n**Respuesta:** " + opciones[random]
                }).ConfigureAwait(false);
            }
            else
            {
                await ctx.RespondAsync("No escribiste las opciones onii-chan" + ctx.User.Mention);
            }
        }

        [Command("sauce"), RequireNsfw]
        public async Task Sauce(CommandContext ctx, string url)
        {
            DiscordMessage msgError = null;
            string msg = "OK";
            if (url.Length > 0)
            {
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    string extension = url.Substring(url.Length - 4);
                    if(extension == ".jpg" || extension == ".png" || extension == "jpeg")
                    {
                        var client = new RestClient("https://trace.moe/api/search?url=" + url);
                        var request = new RestRequest(Method.GET);
                        request.AddHeader("content-type", "application/json");
                        var procesando = await ctx.RespondAsync("Procesando imagen..").ConfigureAwait(false);
                        await ctx.Message.DeleteAsync("Auto borrado de yumiko");
                        IRestResponse response = client.Execute(request);
                        await procesando.DeleteAsync("Auto borrado de yumiko");
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.OK:
                                var resp = JsonConvert.DeserializeObject<dynamic>(response.Content);
                                string resultados = "";
                                bool encontro = false;
                                foreach (var resultado in resp.docs)
                                {
                                    string enlace = "https://anilist.co/anime/";
                                    int similaridad = resultado.similarity * 100;
                                    if(similaridad >= 87)
                                    {
                                        encontro = true;
                                        int segundo = resultado.at;
                                        TimeSpan time = TimeSpan.FromSeconds(segundo);
                                        string at = time.ToString(@"mm\:ss");
                                        resultados =
                                            $"Nombre:    [{resultado.title_romaji}]({enlace += resultado.anilist_id})\n" +
                                            $"Similitud: {similaridad}%\n" +
                                            $"Episodio:  {resultado.episode} (Minuto: {at})\n";
                                        break;
                                    }
                                }
                                if (!encontro)
                                {
                                    resultados = "No se han encontrado resultados para esta imagen.\nRecuerda que solamente funciona con imágenes que sean partes de un episodio";
                                }
                                var embed = new DiscordEmbedBuilder
                                {
                                    Footer = funciones.GetFooter(ctx, "sauce"),
                                    Color = new DiscordColor(78, 63, 96),
                                    Title = "Sauce (Trace.moe)",
                                    ImageUrl = url
                                };
                                embed.AddField("El posible anime de la imagen es", resultados);
                                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                                break;
                            case HttpStatusCode.BadRequest:
                                msg = "Debes ingresar un link";
                                break;
                            case HttpStatusCode.Forbidden:
                                msg = "Acceso denegado";
                                break;
                            case HttpStatusCode.TooManyRequests:
                                msg = "Ratelimit excedido";
                                break;
                            case HttpStatusCode.InternalServerError:
                            case HttpStatusCode.ServiceUnavailable:
                                msg = "Error interno en el servidor de Trace.moe";
                                break;
                            default:
                                msg = "Error inesperado";
                                break;
                        }
                    }
                    else
                    {
                        msg = "La imagen debe ser JPG, PNG o JPEG";
                    }
                }
                else
                {
                    msg = "Debes ingresar el link de una imagen";
                }
            }
            if (msg != "OK")
            {
                msgError = await ctx.RespondAsync(msg).ConfigureAwait(false);
                await Task.Delay(3000);
                await msgError.DeleteAsync("Auto borrado de yumiko").ConfigureAwait(false);
                await ctx.Message.DeleteAsync("Auto borrado de yumiko").ConfigureAwait(false);
            }
        }
        
        [Command("invite"), Aliases("invitar")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.RespondAsync("Puedes invitarme a un servidor con este link:\n" + ConfigurationManager.AppSettings["Invite"]);
            await ctx.Message.DeleteAsync("Auto borrado de yumiko").ConfigureAwait(false);
        }
    }
}
