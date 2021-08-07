using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Anilist : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();

        [Command("anilist"), Aliases("user"), Description("Busca un perfil de AniList."), Hidden]
        public async Task Profile(CommandContext ctx)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("anime"), Description("Busco un anime en AniList"), Hidden]
        public async Task Anime(CommandContext ctx)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("manga"), Description("Busco un manga en AniList"), Hidden]
        public async Task Manga(CommandContext ctx)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("character"), Aliases("personaje"), Description("Busco un personaje en AniList"), Hidden]
        public async Task Character(CommandContext ctx)
        {
            await funciones.MovidoASlashCommand(ctx);
        }
        
        // Staff, algun dia

        [Command("sauce"), Description("Busca el anime de una imagen."), Hidden]
        public async Task Sauce(CommandContext ctx)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("pj"), Description("Personaje aleatorio."), Hidden]
        public async Task Pj(CommandContext ctx)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("AWCRuntime"), Description("Calcula los minutos totales entre animes para AWC."), Hidden]
        public async Task AWCRuntime(CommandContext ctx)
        {
            await funciones.MovidoASlashCommand(ctx);
        }
    }
}
