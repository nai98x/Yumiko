using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using System;
using System.Configuration;
using DSharpPlus.Interactivity.Extensions;

namespace Discord_Bot.Modulos
{
    public class Juegos : BaseCommandModule 
    {
        private readonly FuncionesAuxiliares funciones = new();
        private readonly FuncionesJuegos funcionesJuegos = new();

        [Command("quiz"), Description("Empieza el juego de adivinar algo relacionado con el anime."), RequireGuild, Cooldown(1, 60, CooldownBucketType.Guild), Hidden]
        public async Task QuizGeneral(CommandContext ctx)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("ahorcado"), Description("Empieza el juego del ahorcado de algo relacionado con el anime."), RequireGuild, Hidden]
        public async Task AhorcadoGeneral(CommandContext ctx)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("ranking"), Aliases("stats", "leaderboard"), Description("Estadisticas de un quiz."), RequireGuild, Hidden]
        public async Task EstadisticasGenerales(CommandContext ctx)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("eliminarestadisticas"), Description("Elimina las estadisticas de todos los juegos del servidor."), RequireGuild] // AGREGARLE BOTONES
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
                        await funcionesJuegos.EliminarEstadisticas(ctx);
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
