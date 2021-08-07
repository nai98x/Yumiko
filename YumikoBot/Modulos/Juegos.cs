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

        [Command("eliminarestadisticas"), Description("Elimina las estadisticas de todos los juegos del servidor."), RequireGuild, Hidden]
        public async Task EliminarEstadisticas(CommandContext ctx)
        {
            var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Comando movido a /",
                Description = $"Para jugar ingresa `/deletestats` en vez de `{ctx.Prefix}{ctx.Command.Name}`.",
                Footer = funciones.GetFooter(ctx),
                Color = DiscordColor.Red,
            });
            await Task.Delay(10000);
            await funciones.BorrarMensaje(ctx, msgError.Id);
        }
    }
}
