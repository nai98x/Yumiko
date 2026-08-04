using System.Globalization;

namespace Yumiko.Bot.Localization;

/// <summary>
/// Localizador ya atado a una cultura, creado a partir del locale de la interacción. Es un struct
/// para que se capture por valor dentro de los <c>Task.Run</c> de los juegos.
/// </summary>
public readonly struct Loc(ILocalizer localizer, CultureInfo culture)
{
    public CultureInfo Culture { get; } = culture;

    public string this[string key] => localizer.Get(key, Culture);

    public string Format(string key, params object?[] args) => localizer.Get(key, Culture, args);

    public bool IsSpanish => Equals(Culture, ResxLocalizer.Spanish);
}
