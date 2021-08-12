using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class OtrosSlashCommands : ApplicationCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();

        [SlashCommand("ping", "Muestra el tiempo de respuesta de Yumiko")]
        public async Task Ping(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = "Latencia",
                Description = "🏓 Pong! `" + ctx.Client.Ping.ToString() + " ms" + "`",
                Color = funciones.GetColor()
            }));
        }
    }
}
