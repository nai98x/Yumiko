using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace Discord_Bot.Modulos
{
    public class Help : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("help")]
        [Aliases("ayuda")]
        [Description("Ayuda wey")]
        public async Task Ayuda(CommandContext ctx, [RemainingText]string comando)
        {
            string ayuda;
            if (comando == null)
            {
                ayuda =
                "El prefix del server es `"+ ConfigurationManager.AppSettings["Prefix"] + "`\n" +
                "\n" +
                "Interactuar\n" +
                "`say`, `tts`, `pregunta`, `elegir`\n" +
                "\n" +
                "Memes:\n" +
                "`eli`, `meme`, `mutear`, `waifu`\n" +
                "\n" +
                "Musica:\n" +
                "`join`, `leave`, `play`, `pause`, `resume`, `stop`, `archivos`\n" +
                "\n" +
                "Otros:\n" +
                "`invite`, `donar`, `ping`, `clear`, `expulsar`";

                await ctx.TriggerTypingAsync();

                DiscordEmbed embed = new DiscordEmbedBuilder
                {
                    Title = "Guia de comandos",
                    Description = ayuda,
                    Color = DiscordColor.Purple
                };
                await ctx.RespondAsync(null, false, embed);
            }
            else
            {
                bool ok = true;
                string descripcion="";
                string aliases="";
                string uso = "";
                switch (comando)
                {
                    case "say":
                        descripcion = "El bot reenvia tu mensaje eliminándolo después";
                        aliases = "`s`";
                        uso = "say Hola onii-chan";
                        break;
                    case "tts":
                        descripcion = "Te habla la waifu";
                        uso = "tts Hola onee-chan";
                        break;
                    case "pregunta":
                        descripcion = "Responde con SIS O NON";
                        aliases = "`p`, `question`, `sisonon`";
                        uso = "pregunta ¿Sos mi waifu?";
                        break;
                    case "elegir":
                        descripcion = "Elige entre varias opciones";
                        aliases = "`e`";
                        uso = "elegir ¿Quién es mas puto?\n" +
                              "(respuesta de Yumiko)\n" +
                              "Eli Sadi Nai";
                        break;
                    case "eli":
                        descripcion = "Legendary meme";
                        uso = "eli";
                        break;
                    case "meme":
                        descripcion = "It's a fucking meme";
                        uso = "meme";
                        break;
                    case "mutear":
                        descripcion = "Mutea a un miembro aleatorio del canal (5 minutos de cooldown)";
                        aliases = "`f`";
                        uso = "mutear";
                        break;
                    case "join":
                        descripcion = "Entra Yumiko al canal en el que estés";
                        aliases = "`entrar`";
                        uso = "join";
                        break;
                    case "leave":
                        descripcion = "Sale Yumiko del canal";
                        aliases = "`salir`";
                        uso = "leave";
                        break;
                    case "play":
                        descripcion = "Yumiko reproduce un archivo de audio";
                        uso = "play nombre del archivo";
                        break;
                    case "pause":
                        descripcion = "Yumiko pausa la reproducción de audio";
                        uso = "pause";
                        break;
                    case "resume":
                        descripcion = "Yumiko resume la reproducción de audio";
                        uso = "resume";
                        break;
                    case "stop":
                        descripcion = "Yumiko deja de reproducir audio";
                        uso = "stop";
                        break;
                    case "archivos":
                        descripcion = "Se obtiene una lista de archivos disponibles para reproducir audio";
                        uso = "archivos";
                        break;
                    case "invite":
                        descripcion = "Obtiene enlace para invitar a Yumiko a otros servidores";
                        uso = "invite";
                        break;
                    case "donar":
                        descripcion = "Obtiene enlace para donar al creador de Yumiko";
                        uso = "donar";
                        break;
                    case "ping":
                        descripcion = "Obtiene el ping de Yumiko";
                        uso = "ping";
                        break;
                    case "clear":
                        descripcion = "Elimina una cantidad de mensajes";
                        aliases = "`c`, `borrar`";
                        uso = "clear 3";
                        break;
                    case "expulsar":
                        DiscordMember eli = await ctx.Guild.GetMemberAsync(487779690468212746);
                        descripcion = "Expulsa a un miembro del servidor";
                        aliases = "`kick`";
                        uso = "expulsar " + eli.DisplayName;
                        break;
                    case "waifu":
                        descripcion = "Te dice mi nivel de waifu";
                        uso = "waifu";
                        break;
                    default:
                        ok = false;
                        break;
                }
                if (ok)
                {
                    if(aliases.Length > 0)
                    {
                        ayuda = descripcion + "\n" +
                        "\n" +
                        "**Prefixes alternativos:** " + aliases + "\n" +
                        "\n" +
                        "**Uso:** `" + ConfigurationManager.AppSettings["Prefix"] + uso + "`";
                    }
                    else
                    {
                        ayuda = descripcion + "\n" +
                        "\n" +
                        "**Uso:** `" + ConfigurationManager.AppSettings["Prefix"] + uso + "`";
                    }
                    
                    await ctx.TriggerTypingAsync();
                    DiscordEmbed embed = new DiscordEmbedBuilder
                    {
                        Title = "Guia de comandos | " + comando,
                        Description = ayuda,
                        Color = DiscordColor.Purple
                    };
                    await ctx.RespondAsync(null, false, embed);
                }
                else
                {
                    ayuda = "Comando inválido";
                    await ctx.TriggerTypingAsync();
                    DiscordEmbed embed = new DiscordEmbedBuilder
                    {
                        Title = "Guia de comandos",
                        Description = ayuda,
                        Color = DiscordColor.Purple
                    };
                    await ctx.RespondAsync(null, false, embed);
                }
            }
        }
    }
}
