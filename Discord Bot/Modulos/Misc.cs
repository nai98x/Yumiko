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
            var client = new RestClient("https://trace.moe/api/search?url=" + url);
            var request = new RestRequest(Method.GET);
            request.AddHeader("content-type", "application/json");
            await ctx.RespondAsync("Procesando imagen..").ConfigureAwait(false);
            await ctx.Message.DeleteAsync("Auto borrado de yumiko");
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string resultados = "Los posibles animes de la imagen son:\n\n";
                var resp = JsonConvert.DeserializeObject<dynamic>(response.Content);
                foreach(var result in resp.docs)
                {
                    string enlace = "https://anilist.co/anime/";
                    resultados += $"[{result.title_romaji}]({enlace += result.anilist_id}) - Similitud: {result.similarity}\n";
                }
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx, "sauce"),
                    Color = new DiscordColor(78, 63, 96),
                    Title = "Sauce (Trace.moe)",
                    Description = $"{resultados}",
                    ImageUrl = url
                }).ConfigureAwait(false);
            }
            else
            {
                var msg = await ctx.RespondAsync("Error inesperado").ConfigureAwait(false);
            }
        }
        
        [Command("invite"), Aliases("invitar")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.RespondAsync("Puedes invitarme a un servidor con este link:\n" + ConfigurationManager.AppSettings["Invite"]);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }
    }
}
