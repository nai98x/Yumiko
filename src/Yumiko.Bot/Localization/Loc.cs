using System.Globalization;

namespace Yumiko.Bot.Localization;

/// <summary>
/// Localizer already bound to a culture, created from the locale of the interaction. It is a struct
/// so it is captured by value inside the <c>Task.Run</c> of the games.
/// </summary>
public readonly struct Loc(ILocalizer localizer, CultureInfo culture)
{
    public CultureInfo Culture { get; } = culture;

    public string this[string key] => localizer.Get(key, Culture);

    public string Format(string key, params object?[] args) => localizer.Get(key, Culture, args);

    public bool IsSpanish => Equals(Culture, ResxLocalizer.Spanish);
}
