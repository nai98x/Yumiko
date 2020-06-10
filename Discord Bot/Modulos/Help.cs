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
                "El prefix del servidor es `"+ ConfigurationManager.AppSettings["Prefix"] + "`\n" +
                "Links de utilidad: [Invitación](" + ConfigurationManager.AppSettings["Invite"] + "), [Donar](" + ConfigurationManager.AppSettings["Donar"] + ")";

                await ctx.TriggerTypingAsync();

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Guia de comandos",
                    Description = ayuda,
                    Color = new DiscordColor(78, 63, 96)
                };
                embed.AddField("✍️ Interactuar", "`say`, `tts`, `pregunta`, `elegir`");
                embed.AddField("😂 Memes", "`eli`, `meme`, `mutear`, `waifu`, `earrape`");
                embed.AddField("🎵 Musica", "`join`, `leave`, `play`, `playfile`, `pause`, `resume`, `skip`, `stop`, `nowplaying`, `queue`, `volume`, `seek`, `equializer`, `archivos`");
                embed.AddField("☕️ Otros", "`invite`, `donar`, `ping`, `clear`, `expulsar`, `reiniciar`, `apagar`");
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
                        descripcion = "Yumiko reproduce un video";
                        uso = "play link o busqueda";
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
                        if(eli != null)
                            uso = "expulsar @" + eli.DisplayName;
                        else
                            uso = "expulsar @mencion";
                        break;
                    case "waifu":
                        descripcion = "Te dice mi nivel de waifu";
                        uso = "waifu";
                        break;
                    case "reiniciar":
                        descripcion = "Reinicia a Yumiko";
                        aliases = "`restart`";
                        uso = "reiniciar";
                        break;
                    case "apagar":
                        descripcion = "Apaga a Yumiko";
                        uso = "apagar";
                        break;
                    case "playfile":
                        descripcion = "Yumiko reproduce un archivo de audio";
                        uso = "playfile nombre del archivo";
                        break;
                    case "nowplaying":
                        descripcion = "Yumiko te dice que se está reproduciendo";
                        aliases = "`np`";
                        uso = "nowplaying";
                        break;
                    case "equializer":
                        descripcion = "Se cambia el ecualizador del audio";
                        aliases = "`eq`";
                        uso = "equializer band gain ó equializer (para resetear)";
                        break;
                    case "volume":
                        descripcion = "Cambia el volumen de la reproducción de audio";
                        uso = "volume 50";
                        break;
                    case "seek":
                        descripcion = "Posiciona el reproductor en un tiempo dado";
                        uso = "seek hh:mm:ss";
                        break;
                    case "earrape":
                        descripcion = "EARRAPE por 5 segundos en el reproductor";
                        uso = "earrape";
                        break;
                    case "skip":
                        descripcion = "Yumiko salta esta musicota";
                        uso = "skip";
                        break;
                    case "queue":
                        descripcion = "Se muestra la cola de reproduccion";
                        uso = "queue";
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
                        Color = new DiscordColor(78, 63, 96)
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
                        Color = new DiscordColor(78, 63, 96)
                    };
                    await ctx.RespondAsync(null, false, embed);
                }
            }
        }
    }
}
