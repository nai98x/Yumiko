using Discord_Bot;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace YumikoBot
{
    public class SlashCommands : SlashCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();
        private readonly FuncionesJuegos funcionesJuegos = new();

        [SlashCommand("avatar", "Obtiene un avatar")]
        public async Task Avatar(InteractionContext ctx, [Option("Usuario", "El usuario del avatar")] DiscordUser usuario = null, [Option("Secreto", "Si quieres ver solo tu el comando")] bool secreto = true)
        {
            usuario ??= ctx.Member;
            DiscordMember member = (DiscordMember)usuario;

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Avatar de {member.DisplayName}",
                ImageUrl = usuario.AvatarUrl,
                Footer = funciones.GetFooter(ctx)
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                IsEphemeral = secreto
            }.AddEmbed(embed));
        }

        [SlashCommand("quiz", "Empieza un juego de adivinar algo de anime")]
        public async Task Quiz(InteractionContext ctx,
        [Choice("Adivina el personaje", "personajes")]
        [Choice("Adivina el anime", "animes")]
        [Choice("Adivina el manga", "mangas")]
        [Choice("Adivina el tag", "tag")]
        [Choice("Adivina el estudio", "estudio")]
        [Choice("Adivina el progragonista", "protagonista")]
        [Choice("Adivina el genero", "genero")]
        [Option("Juego", "El tipo de quiz que quieres iniciar")] string juego)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                IsEphemeral = true,
                Content = $"¡Haz iniciado un nuevo quiz!"
            });

            var context = funciones.GetContext(ctx);
            switch (juego)
            {
                case "personajes":
                    await funcionesJuegos.QuizCharactersGlobal(context);
                    break;
                case "animes":
                    await funcionesJuegos.QuizAnimeGlobal(context);
                    break;
                case "mangas":
                    await funcionesJuegos.QuizMangaGlobal(context);
                    break;
                case "tag":
                    await funcionesJuegos.QuizAnimeTagGlobal(context);
                    break;
                case "estudio":
                    await funcionesJuegos.QuizStudioGlobal(context);
                    break;
                case "protagonista":
                    await funcionesJuegos.QuizProtagonistGlobal(context);
                    break;
                case "genero":
                    await funcionesJuegos.QuizGenreGlobal(context);
                    break;
            }
        }

        [SlashCommand("stats", "Busca las estadisticas de un juego de adivinar algo de anime")]
        public async Task Stats(InteractionContext ctx,
        [Choice("Adivina el personaje", "personajes")]
        [Choice("Adivina el anime", "animes")]
        [Choice("Adivina el manga", "mangas")]
        [Choice("Adivina el tag", "tag")]
        [Choice("Adivina el estudio", "estudio")]
        [Choice("Adivina el progragonista", "protagonista")]
        [Choice("Adivina el genero", "genero")]
        [Option("Juego", "El tipo de quiz que quieres ver las estadisticas")] string juego)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                IsEphemeral = true,
                Content = $"¡Haz iniciado la búsqueda de estadisticas!"
            });

            var context = funciones.GetContext(ctx);
            var interactivity = ctx.Client.GetInteractivity();
            DiscordEmbedBuilder builder;
            switch (juego)
            {
                case "personajes":
                    builder = await funcionesJuegos.GetEstadisticas(context, "personaje");
                    await ctx.Channel.SendMessageAsync(embed: builder);
                    await funciones.ChequearVotoTopGG(context);
                    break;
                case "animes":
                    builder = await funcionesJuegos.GetEstadisticas(context, "anime");
                    await ctx.Channel.SendMessageAsync(embed: builder);
                    await funciones.ChequearVotoTopGG(context);
                    break;
                case "mangas":
                    builder = await funcionesJuegos.GetEstadisticas(context, "manga");
                    await ctx.Channel.SendMessageAsync(embed: builder);
                    await funciones.ChequearVotoTopGG(context);
                    break;
                case "tag":
                    builder = await funcionesJuegos.GetEstadisticasTag(context);
                    var msg = await ctx.Channel.SendMessageAsync(embed: builder);
                    await funciones.ChequearVotoTopGG(context);
                    if (builder.Title == "Error")
                    {
                        await Task.Delay(5000);
                        await funciones.BorrarMensaje(context, msg.Id);
                    }
                    break;
                case "estudio":
                    builder = await funcionesJuegos.GetEstadisticas(context, "estudio");
                    await ctx.Channel.SendMessageAsync(embed: builder);
                    await funciones.ChequearVotoTopGG(context);
                    break;
                case "protagonista":
                    builder = await funcionesJuegos.GetEstadisticas(context, "protagonista");
                    await ctx.Channel.SendMessageAsync(embed: builder);
                    await funciones.ChequearVotoTopGG(context);
                    break;
                case "genero":
                    var respuesta = await funcionesJuegos.ElegirGenero(context, interactivity);
                    if (respuesta.Ok)
                    {
                        builder = await funcionesJuegos.GetEstadisticasGenero(context, respuesta.Genero);
                        await ctx.Channel.SendMessageAsync(embed: builder);
                        await funciones.ChequearVotoTopGG(context);
                    }
                    break;
            }
        }
    }
}
