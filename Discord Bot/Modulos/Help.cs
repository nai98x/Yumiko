using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Configuration;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Help : BaseCommandModule
    {
        [Command("help"), Aliases("ayuda")]
        public async Task Ayuda(CommandContext ctx)
        {
            await ctx.RespondAsync("Comandos en:\n" + ConfigurationManager.AppSettings["Web"] + "#commands");
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }
    }
}
