using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus.Entities;
using System;
using GraphQL.Client.Http;
using GraphQL;
using GraphQL.Client.Serializer.Newtonsoft;
using System.Linq;
using System.Configuration;
using DSharpPlus.Interactivity.Extensions;

namespace Discord_Bot.Modulos
{
    public class Estadisticas : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private readonly FuncionesJuegos funcionesJuegos = new FuncionesJuegos();

        [Command("rankingC"), Aliases("statsC", "leaderboardC"), Description("Estadisticas de adivina el personaje."), RequireGuild]
        public async Task EstadisticasAdivinaPersonaje(CommandContext ctx, string flag = null)
        {
            var builder = await funcionesJuegos.GetEstadisticas(ctx, "personaje", flag);
            await ctx.Channel.SendMessageAsync(embed: builder);
            await funciones.ChequearVotoTopGG(ctx);
        }

        [Command("rankingA"), Aliases("statsA", "leaderboardA"), Description("Estadisticas de adivina el anime."), RequireGuild]
        public async Task EstadisticasAdivinaAnime(CommandContext ctx, string flag = null)
        {
            var builder = await funcionesJuegos.GetEstadisticas(ctx, "anime", flag);
            await ctx.Channel.SendMessageAsync(embed: builder);
            await funciones.ChequearVotoTopGG(ctx);
        }

        [Command("rankingM"), Aliases("statsM", "leaderboardM"), Description("Estadisticas de adivina el anime."), RequireGuild]
        public async Task EstadisticasAdivinaManga(CommandContext ctx, string flag = null)
        {
            var builder = await funcionesJuegos.GetEstadisticas(ctx, "manga", flag);
            await ctx.Channel.SendMessageAsync(embed: builder);
            await funciones.ChequearVotoTopGG(ctx);
        }

        [Command("rankingT"), Aliases("statsT", "leaderboardT"), Description("Estadisticas de adivina el anime."), RequireGuild]
        public async Task EstadisticasAdivinaTag(CommandContext ctx, string flag = null)
        {
            var builder = await funcionesJuegos.GetEstadisticasTag(ctx, flag);
            var msg = await ctx.Channel.SendMessageAsync(embed: builder);
            await funciones.ChequearVotoTopGG(ctx);
            if (builder.Title == "Error")
            {
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, msg.Id);
            }
        }

        [Command("rankingS"), Aliases("statsS", "leaderboardS"), Description("Estadisticas de adivina el estudio."), RequireGuild]
        public async Task EstadisticasAdivinaEstudio(CommandContext ctx, string flag = null)
        {
            var builder = await funcionesJuegos.GetEstadisticas(ctx, "estudio", flag);
            await ctx.Channel.SendMessageAsync(embed: builder);
            await funciones.ChequearVotoTopGG(ctx);
        }

        [Command("rankingP"), Aliases("statsP", "leaderboardP"), Description("Estadisticas de adivina el protagonista."), RequireGuild]
        public async Task EstadisticasAdivinaProtagonista(CommandContext ctx, string flag = null)
        {
            var builder = await funcionesJuegos.GetEstadisticas(ctx, "protagonista", flag);
            await ctx.Channel.SendMessageAsync(embed: builder);
            await funciones.ChequearVotoTopGG(ctx);
        }

        //[Command("rankingaP"), Aliases("statsaP", "leaderboardaP"), Description("Estadisticas del ahorcado con personajes."), RequireGuild]
        //public async Task EstadisticasAhorcadoPersonaje(CommandContext ctx, string flag = null)
        //{
        //    var builder = await funcionesJuegos.GetEstadisticas(ctx, "ahorcadoc", flag);
        //    await ctx.Channel.SendMessageAsync(embed: builder);
        //    await funciones.ChequearVotoTopGG(ctx);
        //}

        [Command("eliminarestadisticas"), Description("Elimina las estadisticas de todos los juegos del servidor."), RequireGuild]
        public async Task EliminarEstadisticas(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            string opcion;
            string opciones =
                $"**1-** Si\n" +
                $"**2-** No\n\n" +
                $"**Ten en cuenta que el borrado de estadisticas no se puede deshacer.**";
            var msgElegir = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Confirma si quieres eliminar tus estadisticas del servidor",
                Description = opciones,
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
            });
            var msgElegirInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));
            if (!msgElegirInter.TimedOut)
            {
                opcion = msgElegirInter.Result.Content;
                if (msgElegir != null)
                    await funciones.BorrarMensaje(ctx, msgElegir.Id);
                if (msgElegirInter.Result != null)
                    await funciones.BorrarMensaje(ctx, msgElegirInter.Result.Id);
                opcion = opcion.ToLower();
                switch (opcion)
                {
                    case "1":
                    case "1- si":
                    case "si":
                        await funcionesJuegos.ElimianrEstadisticas(ctx);
                        break;
                    case "2":
                    case "2- no":
                    case "no":
                        break;
                    default:
                        var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "Error",
                            Description = "Opcion incorrecta",
                            Footer = funciones.GetFooter(ctx),
                            Color = DiscordColor.Red,
                        });
                        await Task.Delay(3000);
                        if (msgError != null)
                            await funciones.BorrarMensaje(ctx, msgError.Id);
                        break;
                }
            }
            if (msgElegir != null)
                await funciones.BorrarMensaje(ctx, msgElegir.Id);
            if (msgElegirInter.Result != null)
                await funciones.BorrarMensaje(ctx, msgElegirInter.Result.Id);
        }
    }
}
