using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Help : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private string Descripcion { get; set; }
        private string Aliases { get; set; }
        private string Uso { get; set; }
        private bool Ok { get; set; }

        [Command("help")]
        [Aliases("ayuda")]
        [Description("Ayuda wey")]
        public async Task Ayuda(CommandContext ctx, [RemainingText]string comando)
        {
            if (comando == null)
            {
                await ImprimirNormal(ctx);
            }
            else
            {
                string ayuda;
                SetCampos(comando);
                if (Ok)
                {
                    if(Aliases.Length > 0)
                    {
                        ayuda = Descripcion + "\n" +
                        "\n" +
                        "**Prefixes alternativos:** " + Aliases + "\n" +
                        "\n" +
                        "**Uso:** `" + ConfigurationManager.AppSettings["Prefix"] + Uso + "`";
                    }
                    else
                    {
                        ayuda = Descripcion + "\n" +
                        "\n" +
                        "**Uso:** `" + ConfigurationManager.AppSettings["Prefix"] + Uso + "`";
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

        private async Task ImprimirNormal(CommandContext ctx)
        {
            string ayuda =
                "El prefix del servidor es `" + ConfigurationManager.AppSettings["Prefix"] + "`\n" +
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

        private void SetCampos(string comando)
        {
            Ok = true;
            switch (comando)
            {
                case "say":
                    Descripcion = "El bot reenvia tu mensaje eliminándolo después";
                    Aliases = "`s`";
                    Uso = "say Hola onii-chan";
                    break;
                case "tts":
                    Descripcion = "Te habla la waifu";
                    Uso = "tts Hola onee-chan";
                    break;
                case "pregunta":
                    Descripcion = "Responde con SIS O NON";
                    Aliases = "`p`, `question`, `sisonon`";
                    Uso = "pregunta ¿Sos mi waifu?";
                    break;
                case "elegir":
                    Descripcion = "Elige entre varias opciones";
                    Aliases = "`e`";
                    Uso = "elegir ¿Quién es mas puto?\n" +
                          "(respuesta de Yumiko)\n" +
                          "Eli Sadi Nai";
                    break;
                case "eli":
                    Descripcion = "Legendary meme";
                    Uso = "eli";
                    break;
                case "meme":
                    Descripcion = "It's a fucking meme";
                    Uso = "meme";
                    break;
                case "mutear":
                    Descripcion = "Mutea a un miembro aleatorio del canal (5 minutos de cooldown)";
                    Aliases = "`f`";
                    Uso = "mutear";
                    break;
                case "join":
                    Descripcion = "Entra Yumiko al canal en el que estés";
                    Aliases = "`entrar`";
                    Uso = "join";
                    break;
                case "leave":
                    Descripcion = "Sale Yumiko del canal";
                    Aliases = "`salir`";
                    Uso = "leave";
                    break;
                case "play":
                    Descripcion = "Yumiko reproduce un video";
                    Uso = "play link o busqueda";
                    break;
                case "pause":
                    Descripcion = "Yumiko pausa la reproducción de audio";
                    Uso = "pause";
                    break;
                case "resume":
                    Descripcion = "Yumiko resume la reproducción de audio";
                    Uso = "resume";
                    break;
                case "stop":
                    Descripcion = "Yumiko deja de reproducir audio";
                    Uso = "stop";
                    break;
                case "archivos":
                    Descripcion = "Se obtiene una lista de archivos disponibles para reproducir audio";
                    Uso = "archivos";
                    break;
                case "invite":
                    Descripcion = "Obtiene enlace para invitar a Yumiko a otros servidores";
                    Uso = "invite";
                    break;
                case "donar":
                    Descripcion = "Obtiene enlace para donar al creador de Yumiko";
                    Uso = "donar";
                    break;
                case "ping":
                    Descripcion = "Obtiene el ping de Yumiko";
                    Uso = "ping";
                    break;
                case "clear":
                    Descripcion = "Elimina una cantidad de mensajes";
                    Aliases = "`c`, `borrar`";
                    Uso = "clear 3";
                    break;
                case "expulsar":
                    Descripcion = "Expulsa a un miembro del servidor";
                    Aliases = "`kick`";
                    Uso = "expulsar @mencion";
                    break;
                case "waifu":
                    Descripcion = "Te dice mi nivel de waifu";
                    Uso = "waifu";
                    break;
                case "reiniciar":
                    Descripcion = "Reinicia a Yumiko";
                    Aliases = "`restart`";
                    Uso = "reiniciar";
                    break;
                case "apagar":
                    Descripcion = "Apaga a Yumiko";
                    Uso = "apagar";
                    break;
                case "playfile":
                    Descripcion = "Yumiko reproduce un archivo de audio";
                    Uso = "playfile nombre del archivo";
                    break;
                case "nowplaying":
                    Descripcion = "Yumiko te dice que se está reproduciendo";
                    Aliases = "`np`";
                    Uso = "nowplaying";
                    break;
                case "equializer":
                    Descripcion = "Se cambia el ecualizador del audio";
                    Aliases = "`eq`";
                    Uso = "equializer band gain ó equializer (para resetear)";
                    break;
                case "volume":
                    Descripcion = "Cambia el volumen de la reproducción de audio";
                    Uso = "volume 50";
                    break;
                case "seek":
                    Descripcion = "Posiciona el reproductor en un tiempo dado";
                    Uso = "seek hh:mm:ss";
                    break;
                case "earrape":
                    Descripcion = "EARRAPE por 5 segundos en el reproductor";
                    Uso = "earrape";
                    break;
                case "skip":
                    Descripcion = "Yumiko salta esta musicota";
                    Uso = "skip";
                    break;
                case "queue":
                    Descripcion = "Se muestra la cola de reproduccion";
                    Uso = "queue";
                    break;
                default:
                    Ok = false;
                    break;
            }
        }
    }
}
