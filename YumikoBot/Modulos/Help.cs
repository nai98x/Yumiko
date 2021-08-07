using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Help : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();

        [Command("help"), Aliases("ayuda", "commands", "comandos")]
        public async Task Ayuda(CommandContext ctx, [Description("Comando para ver en detalle, si se deja vacío se muestran todos los comandos")] string comando = null)
        {
            var commandsNext = ctx.CommandsNext;

            var comandos = commandsNext.RegisteredCommands.Values;
            string web = ConfigurationManager.AppSettings["Web"] + "#commands";
            string urlTopGG = "https://top.gg/bot/295182825521545218/";
            var comandosFiltrados = from com in comandos
                                    group com by com.Module.ModuleType.Name;
            if (comando == null)
            {
                string comandosDesc = string.Empty;
                var builder = new DiscordEmbedBuilder
                {
                    Title = "Comandos disponibles",
                    Description = $"Puedes llamarme con `{ctx.Prefix}` o con {ctx.Client.CurrentUser.Mention}\n" +
                    $"[Ejemplos de comandos]({web}) | [¡Vótame!]({urlTopGG})\n\n" +
                    $"**Importante:** Para ver el resto de comandos, escribe `/help`",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor()
                };
                foreach (var grp in comandosFiltrados)
                {
                    var grupo = grp.Distinct();
                    string nomGrupo = grp.Key;
                    List<string> listaComandos = new();
                    foreach (var comando1 in grupo)
                    {
                        if (!comando1.IsHidden)
                        {
                            string nomComando = comando1.Name;
                            listaComandos.Add($"`{nomComando}`");
                        } 
                    }
                    comandosDesc = string.Join(", ", listaComandos);
                    if (nomGrupo == "NSFW" && !ctx.Channel.IsNSFW)
                        comandosDesc = "`Para ver estos comandos ejecutalo en un canal NSFW`";
                    if(nomGrupo != "Help" && ! string.IsNullOrEmpty(comandosDesc))
                    {
                        builder.AddField(nomGrupo, comandosDesc, false);
                    }
                }
                await ctx.Channel.SendMessageAsync(embed: builder).ConfigureAwait(false);
            }
            else
            {
                var comanditos = comandos.Distinct();
                var listaComandos = comanditos.ToList();
                Command comandoEncontrado = listaComandos.Find(x => x.Name == comando || x.Aliases.Contains(comando));
                if(comandoEncontrado != null && !comandoEncontrado.IsHidden)
                {
                    _ = await funciones.GetInfoComando(ctx, comandoEncontrado);
                }
                else
                {
                    var keys = from com in comandosFiltrados
                               select com.Key;
                    var keysList = keys.ToList();
                    string categoria = keysList.Find(x => x.ToLower().Trim() == comando.ToLower().Trim());
                    if (categoria != null)
                    {
                        string comandosDesc = string.Empty;
                        foreach (var grp in comandosFiltrados)
                        {
                            var grupo = grp.Distinct();
                            string nomGrupo = grp.Key;
                            if (nomGrupo == categoria)
                            {
                                List<string> listaComandos1 = new();
                                foreach (var comando1 in grupo)
                                {
                                    if (!comando1.IsHidden)
                                        listaComandos1.Add($"`{comando1.Name}`");
                                }
                                comandosDesc = string.Join(", ", listaComandos1);
                                if (nomGrupo == "NSFW" && !ctx.Channel.IsNSFW)
                                    comandosDesc = "`Para ver estos comandos ejecutalo en un canal NSFW`";
                            }
                        }
                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = categoria,
                            Description = comandosDesc,
                            Url = ConfigurationManager.AppSettings["Web"] + "#" + categoria,
                            Footer = funciones.GetFooter(ctx),
                            Color = funciones.GetColor()
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        var msgError = await ctx.Channel.SendMessageAsync($"No se ha encontrado el comando `{comando}`").ConfigureAwait(false);
                        await Task.Delay(3000);
                        await funciones.BorrarMensaje(ctx, msgError.Id);
                    }
                }
            }
        }
    }
}
