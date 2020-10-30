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
        public async Task Ayuda(CommandContext ctx, string comando = null)
        {
            var commandsNext = ctx.CommandsNext;
            var comandos = commandsNext.RegisteredCommands.Values;
            string web = ConfigurationManager.AppSettings["Web"] + "#commands";
            if (comando == null)
            {
                var comandosFiltrados = from com in comandos
                                        group com by com.Module.ModuleType.Name;
                string comandosDesc = "";
                var builder = new DiscordEmbedBuilder
                {
                    Title = "Comandos disponibles",
                    Description = $"Puedes llamarme con `{ConfigurationManager.AppSettings["Prefix"]}`, con `yumiko` o con {ctx.Client.CurrentUser.Mention}.\nSi quieres ver ejemplos puedes visitar mi [página web]({web}).",
                    Url = web,
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor()
                };
                foreach (var grp in comandosFiltrados)
                {
                    comandosDesc = "";
                    var grupo = grp.Distinct();
                    string nomGrupo = grp.Key;
                    foreach (var comando1 in grupo)
                    {
                        comandosDesc += $"`{comando1.Name}` ";
                    }
                    builder.AddField(nomGrupo, comandosDesc, false);
                }
                await ctx.RespondAsync(embed: builder).ConfigureAwait(false);
            }
            else
            {
                var comanditos = comandos.Distinct();
                var listaComandos = comanditos.ToList();
                Command comandoEncontrado = listaComandos.Find(x => x.Name == comando);
                string nomComando = comandoEncontrado.Name;
                if(comandoEncontrado != null)
                {
                    var builder = new DiscordEmbedBuilder
                    {
                        Title = $"Comando {nomComando}",
                        Url = web,
                        Footer = funciones.GetFooter(ctx),
                        Color = funciones.GetColor()
                    };
                    string modulo = comandoEncontrado.Module.ModuleType.Name;
                    var aliases = comandoEncontrado.Aliases;
                    string descripcion = comandoEncontrado.Description;
                    if(modulo != null)
                        builder.AddField("Modulo", modulo, false);
                    if(aliases.Count > 0)
                    {
                        List<string> listaAliases = new List<string>();
                        foreach(string a in aliases)
                        {
                            listaAliases.Add($"`{a}`");
                        }
                        string aliasesC = string.Join(" ", listaAliases);
                        builder.AddField("Aliases", aliasesC, false);
                    }
                    if(descripcion != null)
                        builder.AddField("Descripcion", descripcion, false);
                    foreach (var overload in comandoEncontrado.Overloads) 
                    {
                        string parametros = "";
                        foreach (var argument in overload.Arguments)
                        {
                            if(argument.Description != null)
                                parametros += $":arrow_right: **{argument.Name}** | Descripcion: {argument.Description} | Opcional: {argument.IsOptional}\n";
                            else
                                parametros += $":arrow_right: **{argument.Name}** | Opcional: {argument.IsOptional}\n";
                        }
                        builder.AddField("Parametros", parametros, false);
                    }
                    await ctx.RespondAsync(embed: builder).ConfigureAwait(false);
                }
                else
                {
                    var msgError = await ctx.RespondAsync($"No se ha encontrado el comando `{comando}`").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await msgError.DeleteAsync().ConfigureAwait(false);
                }
            }
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }
    }
}
