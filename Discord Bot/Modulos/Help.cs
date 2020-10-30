using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using GraphQL;
using GraphQL.Language.AST;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Help : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("help"), Aliases("ayuda")]
        public async Task Ayuda(CommandContext ctx)
        {
            var commandsNext = ctx.CommandsNext;
            var comandos = commandsNext.RegisteredCommands.Values;
            var comandosFiltrados = from com in comandos
                           group com by com.Module.ModuleType.Name;
            string comandosDesc = "";
            var builder = new DiscordEmbedBuilder
            {
                Title = "Comandos Disponibles",
                Url = ConfigurationManager.AppSettings["Web"] + "#commands",
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor()
            };
            foreach (var grp in comandosFiltrados)
            {
                comandosDesc = "";
                var grupo = grp.Distinct();
                string nomGrupo = grp.Key;
                foreach (var comando in grupo)
                {
                    comandosDesc += $"`{comando.Name}` ";
                }
                builder.AddField(nomGrupo, comandosDesc, false);
            }
            await ctx.RespondAsync(embed: builder).ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }
    }
}
