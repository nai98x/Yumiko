using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Entities;
using Yumiko.Bot.Localization;

namespace Yumiko.Bot.Commands.Framework;

/// <summary>
/// Traduce nombres y descripciones de comandos con los mismos .resx que el resto de los textos.
/// Se aplica como <c>[InteractionLocalizer&lt;ResxInteractionLocalizer&gt;("clave")]</c>.
/// </summary>
public sealed class ResxInteractionLocalizer(ILocalizer localizer) : IInteractionLocalizer
{
    public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName)
    {
        string spanish = localizer.Get(fullSymbolName, ResxLocalizer.Spanish);

        // Si la clave no existe el localizer devuelve la clave misma: en ese caso no se localiza nada
        // y Discord se queda con el nombre/descripción declarado en el atributo.
        if (spanish == fullSymbolName)
        {
            return ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(
                new Dictionary<DiscordLocale, string>());
        }

        return ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            [DiscordLocale.es_ES] = spanish,
        });
    }
}
