using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Anilist : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();

        [Command("anilist"), Aliases("user"), Description("Busca un perfil de AniList.")]
        public async Task Profile(CommandContext ctx, [Description("El nick del perfil de AniList")]string usuario = null)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("anime"), Description("Busco un anime en AniList")]
        public async Task Anime(CommandContext ctx, [RemainingText][Description("Nombre del anime a buscar")] string anime = null)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("manga"), Description("Busco un manga en AniList")]
        public async Task Manga(CommandContext ctx, [RemainingText][Description("Nombre del manga a buscar")] string manga = null)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("character"), Aliases("personaje"), Description("Busco un personaje en AniList")]
        public async Task Character(CommandContext ctx, [RemainingText][Description("Nombre del personaje a buscar")] string personaje = null)
        {
            await funciones.MovidoASlashCommand(ctx);
        }
        
        // Staff, algun dia

        [Command("sauce"), Description("Busca el anime de una imagen.")]
        public async Task Sauce(CommandContext ctx, [Description("Link de la imagen")] string url = null)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("pj"), Description("Personaje aleatorio.")]
        public async Task Pj(CommandContext ctx)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("AWCRuntime"), Description("Calcula los minutos totales entre animes para AWC.")]
        public async Task AWCRuntime(CommandContext ctx)
        {
            await funciones.MovidoASlashCommand(ctx);
        }
    }
}
