using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Yumiko.Bot.Helpers;
using Yumiko.Model.Entities;

namespace Yumiko.Bot.Commands.Framework.AutoComplete;

public sealed class CountriesAutoCompleteProvider : IAutoCompleteProvider
{
    public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        CountriesCatalog catalog = context.ServiceProvider.GetRequiredService<CountriesCatalog>();
        string? text = context.UserInput?.ToString();

        IEnumerable<DiscordAutoCompleteChoice> options = catalog
            .Search(text)
            .Select(country => new DiscordAutoCompleteChoice(DisplayNameOf(country, context.Interaction.Locale), country.Code));

        return ValueTask.FromResult(options);
    }

    private static string DisplayNameOf(Country country, string? locale) =>
        locale is not null && locale.StartsWith("es", StringComparison.OrdinalIgnoreCase)
            ? country.NameSpanish
            : country.NameEnglish;
}
