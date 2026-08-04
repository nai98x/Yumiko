using System.Globalization;
using System.Resources;

namespace Yumiko.Bot.Localization;

public sealed class ResxLocalizer : ILocalizer
{
    /// <summary>Culturas con traducción propia; el resto cae al neutral (inglés).</summary>
    public static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es");

    public static readonly CultureInfo Neutral = CultureInfo.InvariantCulture;

    private readonly ResourceManager _resources = new("Yumiko.Bot.Resources.Translations", typeof(ResxLocalizer).Assembly);

    public string Get(string key, CultureInfo culture, params object?[] args)
    {
        string? text = _resources.GetString(key, culture);

        if (text is null)
        {
            return key;
        }

        return args.Length == 0 ? text : string.Format(culture, text, args);
    }

    public Loc For(CultureInfo culture) => new(this, culture);

    public Loc For(string? discordLocale) => new(this, Culture(discordLocale));

    /// <summary>
    /// Discord manda locales tipo "es-ES" o "es-419". Cualquier variante de español usa el .resx
    /// español; el resto cae al neutral.
    /// </summary>
    public static CultureInfo Culture(string? discordLocale) =>
        discordLocale is not null && discordLocale.StartsWith("es", StringComparison.OrdinalIgnoreCase)
            ? Spanish
            : Neutral;
}
