using System.Globalization;

namespace Yumiko.Bot.Localization;

public interface ILocalizer
{
    /// <summary>Texto de <paramref name="key"/> en la cultura pedida, formateado con <paramref name="args"/>.</summary>
    string Get(string key, CultureInfo culture, params object?[] args);

    /// <summary>Captura una cultura para no tener que pasarla en cada llamada.</summary>
    Loc For(CultureInfo culture);

    /// <summary>Igual que <see cref="For(CultureInfo)"/> pero a partir del locale que manda Discord.</summary>
    Loc For(string? discordLocale);
}
