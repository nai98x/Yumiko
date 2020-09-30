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
using System.Security.Cryptography.X509Certificates;

namespace Discord_Bot.Modulos
{
    public class Games : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("quiz")]
        [Description("Adivina el personaje")]
        [RequireOwner]
        public async Task QuizCharactersGlobal(CommandContext ctx, int rondas)
        {
            await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
            await ctx.RespondAsync($"Adivina el personaje! Sesión inciada por **{ctx.User.Username}#{ctx.User.Discriminator}**! Rondas: **{rondas}**");
            string query= "query($page: Int) {" +
                "Page(page: $page) {" +
                    "characters(sort: FAVOURITES_DESC){" +
                        "siteUrl," +
                        "name{" +
                            "first," +
                            "full" +
                        "}" +
                        "image{" +
                            "large" +
                        "}" +
                    "}" +
                "}" +
            "}";
            List<Character> characterList = new List<Character>();
            var graphQLClient = new GraphQLClient("https://graphql.anilist.co");
            DiscordMessage mensaje = await ctx.RespondAsync("Recolectando pesonajes...").ConfigureAwait(false);
            for (int i=1; i<=20; i++) // 1000 personajes LIMITE 90 PETICIONES POR MINUTO
            {
                var response = await graphQLClient.QueryAsync(query, new { page = i });
                var data = JsonConvert.DeserializeObject<dynamic>(response);
                foreach (var x in data.data.Page.characters)
                {
                    characterList.Add(new Character() {
                        Image = x.image.large,
                        NameFull = x.name.full,
                        NameFirst = x.name.first,
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
            var thumbsUpEmoji = DiscordEmoji.FromName(ctx.Client, ":+1:");
            var thumbsDownEmoji = DiscordEmoji.FromName(ctx.Client, ":-1:");
            await joinMessage.CreateReactionAsync(thumbsUpEmoji).ConfigureAwait(false);
            var resultado = await interactivity.CollectReactionsAsync(joinMessage, TimeSpan.FromMinutes(1));
            Reaction reaccion = resultado.FirstOrDefault();
            List<UsuarioJuego> participantes = new List<UsuarioJuego>();
            foreach (DiscordUser partic in reaccion.Users)
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
            await interactivity.WaitForReactionAsync(x => x.Message == joinMessage && x.Emoji == thumbsUpEmoji);
            Random rnd = new Random();
            for (int ronda = 1; ronda <= rondas; ronda++)
            {
                int random = rnd.Next(characterList.Count - 1);
                Character elegido = characterList[random];
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Gold,
                    Title = "Adivina el personaje",
                    Description = $"Ronda {ronda} de {rondas}",
                    ImageUrl = elegido.Image
                }).ConfigureAwait(false);
                var msg = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && (xm.Content.ToLower() == elegido.NameFull.ToLower() || xm.Content.ToLower() == elegido.NameFirst.ToLower()), TimeSpan.FromSeconds(20));
                if(!msg.TimedOut)
                {
                    DiscordMember acertador = await ctx.Guild.GetMemberAsync(msg.Result.Author.Id);
                    await ctx.RespondAsync($"**{acertador.DisplayName}** ha acertado!").ConfigureAwait(false);
                    /*if (reaccion.Users.Where(x => x == msg.Result.Author))
                    {

                    }*/
                }
                else
                {
                    await ctx.RespondAsync($"Nadie ha acertado! El nombre era **{elegido.NameFull}**").ConfigureAwait(false);
                }
            }
        }
    }
}
