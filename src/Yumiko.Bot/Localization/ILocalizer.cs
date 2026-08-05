using System.Globalization;

namespace Yumiko.Bot.Localization;

public interface ILocalizer
{
    /// <summary>Text of <paramref name="key"/> in the requested culture, formatted with <paramref name="args"/>.</summary>
    string Get(string key, CultureInfo culture, params object?[] args);

    /// <summary>Captures a culture so it does not have to be passed on every call.</summary>
    Loc For(CultureInfo culture);

    /// <summary>Same as <see cref="For(CultureInfo)"/> but from the locale Discord sends.</summary>
    Loc For(string? discordLocale);
}
