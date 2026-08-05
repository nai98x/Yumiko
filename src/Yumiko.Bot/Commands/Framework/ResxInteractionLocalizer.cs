using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Entities;
using Yumiko.Bot.Localization;

namespace Yumiko.Bot.Commands.Framework;

/// <summary>
/// Translates command names and descriptions with the same .resx files as the rest of the texts.
/// It is applied as <c>[InteractionLocalizer&lt;ResxInteractionLocalizer&gt;("key")]</c>.
/// </summary>
public sealed class ResxInteractionLocalizer(ILocalizer localizer) : IInteractionLocalizer
{
    public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName)
    {
        string spanish = localizer.Get(fullSymbolName, ResxLocalizer.Spanish);

        // If the key does not exist the localizer returns the key itself: in that case nothing is localized
        // and Discord keeps the name/description declared in the attribute.
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
