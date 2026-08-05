using System.Globalization;
using System.Resources;

namespace Yumiko.Bot.Localization;

public sealed class ResxLocalizer : ILocalizer
{
    /// <summary>Cultures with their own translation; the rest falls back to the neutral one (English).</summary>
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
    /// Discord sends locales like "es-ES" or "es-419". Any Spanish variant uses the Spanish
    /// .resx; the rest falls back to the neutral one.
    /// </summary>
    public static CultureInfo Culture(string? discordLocale) =>
        discordLocale is not null && discordLocale.StartsWith("es", StringComparison.OrdinalIgnoreCase)
            ? Spanish
            : Neutral;
}
