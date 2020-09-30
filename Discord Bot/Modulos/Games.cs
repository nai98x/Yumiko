using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using Miki.GraphQL;
using Newtonsoft.Json;
using System.Collections.Generic;
using Miki.Anilist;
using DSharpPlus.Entities;
using System;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Discord_Bot.Modulos
{
    public class Games : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("quiz")]
        [Description("Adivina el personaje")]
        [RequireOwner]
        public async Task QuizCharactersGlobal(CommandContext ctx, int rondas = 0)
        {
            if(rondas <= 0)
            {
                rondas = 10; // Default
            }
            await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
            await ctx.RespondAsync($"Adivina el personaje! Sesión inciada por **{ctx.User.Username}#{ctx.User.Discriminator}**! Rondas: **{rondas}**");
            List<Character> characterList = new List<Character>();
            var graphQLClient = new GraphQLClient("https://graphql.anilist.co");
            Random rnd = new Random();
            int iteraciones = 20;
            DiscordMessage mensaje = await ctx.RespondAsync($"Obteniendo pesonajes...").ConfigureAwait(false);
            for (int i=1; i<=iteraciones; i++) 
            {
                string queryIni = "{" +
                "Page (page: ";
                string queryFin = ") { " +
                    "characters(sort: FAVOURITES_DESC){" +
                        "siteUrl," +
                        "name{" +
                            "first," +
                            "last," +
                            "full" +
                        "}" +
                        "image{" +
                            "large" +
                        "}" +
                    "}" +
                "}" +
                "}";
                string query = queryIni + i + queryFin;
                var response = await graphQLClient.QueryAsync(query, new { page = 1});
                var data = JsonConvert.DeserializeObject<dynamic>(response);
                foreach (var x in data.data.Page.characters)
                {
                    characterList.Add(new Character() {
                        Image = x.image.large,
                        NameFull = x.name.full,
                        NameFirst = x.name.first,
                        NameLast = x.name.last,
                        SiteUrl = x.siteUrl
                    });
                }
            }
            await mensaje.DeleteAsync("Auto borrado de Yumiko");
            var interactivity = ctx.Client.GetInteractivity();
            var joinEmbed = new DiscordEmbedBuilder
            {
                Title= "Adivina el personaje",
                Description = "Reacciona para participar!"
            };
            var joinMessage = await ctx.Channel.SendMessageAsync(embed: joinEmbed).ConfigureAwait(false);
            var emojiReaccion = DiscordEmoji.FromName(ctx.Client, ":pencil:");
            await joinMessage.CreateReactionAsync(emojiReaccion).ConfigureAwait(false);
            var resultado = await interactivity.CollectReactionsAsync(joinMessage, TimeSpan.FromSeconds(20));
            List<UsuarioJuego> participantes = new List<UsuarioJuego>();
            foreach (Reaction rec in resultado)
            {
                if(rec.Emoji == emojiReaccion)
                {
                    foreach (DiscordUser partic in rec.Users)
                    {
                        if (!partic.IsBot)
                        {
                            participantes.Add(new UsuarioJuego()
                            {
                                usuario = partic,
                                puntaje = 0
                            });
                        }
                    }
                }
            }
            string participantes1 = "";
            foreach (UsuarioJuego uj in participantes)
            {
                participantes1 += $"- {uj.usuario.Username}#{uj.usuario.Discriminator}\n";
            }
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
            {
                Title = "Adivina el personaje - Jugadores",
                Description = participantes1
            }).ConfigureAwait(false);
            for (int ronda = 1; ronda <= rondas; ronda++)
            {
                int random = funciones.GetNumeroRandom(0, characterList.Count - 1);
                Character elegido = characterList[random];
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Gold,
                    Title = "Adivina el personaje",
                    Description = $"Ronda {ronda} de {rondas}",
                    ImageUrl = elegido.Image
                }).ConfigureAwait(false);
                var msg = await interactivity.WaitForMessageAsync
                    (xm => xm.Channel == ctx.Channel && 
                    (xm.Content.ToLower() == elegido.NameFull.ToLower() || xm.Content.ToLower() == elegido.NameFirst.ToLower() || (elegido.NameLast != null && xm.Content.ToLower() == elegido.NameLast.ToLower())) &&
                    participantes.Find(x => x.usuario == xm.Author) != null
                    , TimeSpan.FromSeconds(20));
                if(!msg.TimedOut)
                {
                    DiscordMember acertador = await ctx.Guild.GetMemberAsync(msg.Result.Author.Id);
                    UsuarioJuego usr = participantes.Find(x => x.usuario == msg.Result.Author);
                    usr.puntaje++;
                    await ctx.RespondAsync($"**{acertador.DisplayName}** ha acertado! {elegido.NameFull} (puesto en popularidad: {random+1})").ConfigureAwait(false);
                }
                else
                {
                    await ctx.RespondAsync($"Nadie ha acertado! El nombre era **{elegido.NameFull}**").ConfigureAwait(false);
                }
            }
            string resultados = "";
            participantes.OrderBy(x => x.puntaje);
            foreach (UsuarioJuego uj in participantes)
            {
                resultados += $"- {uj.usuario.Username}#{uj.usuario.Discriminator}: {uj.puntaje}\n";
            }
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = "Adivina el personaje - Resultados",
                Description = resultados
            }).ConfigureAwait(false);
        }
    }
}
