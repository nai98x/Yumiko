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
    [RequireBotPermissions(DSharpPlus.Permissions.SendMessages)]
    public class Help : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("help"), Aliases("ayuda", "commands", "comandos")]
        public async Task Ayuda(CommandContext ctx, [Description("Comando para ver en detalle, si se deja vacío se muestran todos los comandos")] string comando = null)
        {
            var commandsNext = ctx.CommandsNext;
            var comandos = commandsNext.RegisteredCommands.Values;
            string web = ConfigurationManager.AppSettings["Web"] + "#commands";
            string urlTopGG = "https://top.gg/bot/295182825521545218/";
            if (comando == null)
            {
                var comandosFiltrados = from com in comandos
                                        group com by com.Module.ModuleType.Name;
                string comandosDesc = "";
                var builder = new DiscordEmbedBuilder
                {
                    Title = "Comandos disponibles",
                    Description = $"Puedes llamarme con `{ctx.Prefix}` o con {ctx.Client.CurrentUser.Mention}.\n" +
                    $"[Ejemplos de comandos]({web}) | [Top.gg]({urlTopGG}) | [¡Vótame!]({urlTopGG}vote)",
                    Url = web,
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor()
                };
                foreach (var grp in comandosFiltrados)
                {
                    var grupo = grp.Distinct();
                    string nomGrupo = grp.Key;
                    List<string> listaComandos = new List<string>();
                    foreach (var comando1 in grupo)
                    {
                        if(!comando1.IsHidden)
                            listaComandos.Add($"`{comando1.Name}`");
                    }
                    comandosDesc = string.Join(", ", listaComandos);
                    if (nomGrupo == "NSFW" && !ctx.Channel.IsNSFW)
                        comandosDesc = "`Para ver estos comandos ejecutalo en un canal NSFW`";
                    builder.AddField(nomGrupo, comandosDesc, false);
                }
                await ctx.RespondAsync(embed: builder).ConfigureAwait(false);
            }
            else
            {
                var comanditos = comandos.Distinct();
                var listaComandos = comanditos.ToList();
                Command comandoEncontrado = listaComandos.Find(x => x.Name == comando || x.Aliases.Contains(comando));
                if(comandoEncontrado != null && !comandoEncontrado.IsHidden)
                {
                    string nomComando = comandoEncontrado.Name;
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
                        string aliasesC = string.Join(", ", listaAliases);
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
                            {
                                if(argument.IsOptional)
                                    parametros += $":arrow_right: **{argument.Name}** | {argument.Description} | Obligatorio: **No**\n";
                                else
                                    parametros += $":arrow_right: **{argument.Name}** | {argument.Description} | Obligatorio: **Si**\n";
                            }
                            else
                            {
                                if(argument.IsOptional)
                                    parametros += $":arrow_right: **{argument.Name}** | Obligatorio: **No**\n";
                                else
                                    parametros += $":arrow_right: **{argument.Name}** | Obligatorio: **Si**\n";
                            }  
                        }
                        if(!string.IsNullOrEmpty(parametros))
                            builder.AddField("Parametros", parametros, false);
                    }
                    await ctx.RespondAsync(embed: builder).ConfigureAwait(false);
                }
                else
                {
                    var msgError = await ctx.RespondAsync($"No se ha encontrado el comando `{comando}`").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(ctx, msgError.Id);
                }
            }
        }
    }
}
