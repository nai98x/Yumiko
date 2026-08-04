using System.Globalization;
using System.Text.RegularExpressions;

namespace Yumiko.Application.Helpers;

/// <summary>Emote personalizado de Discord tal como aparece escrito en un mensaje.</summary>
public sealed record CustomEmoji(string Name, ulong Id, bool Animated)
{
    public string Url => $"https://cdn.discordapp.com/emojis/{Id}.{(Animated ? "gif" : "png")}";

    /// <summary>Fecha embebida en el snowflake (época de Discord: 2015-01-01).</summary>
    public DateTimeOffset CreationTimestamp =>
        DateTimeOffset.FromUnixTimeMilliseconds((long)(Id >> 22) + 1420070400000L);
}

public static partial class EmojiHelper
{
    /// <summary>
    /// Reconoce las formas <c>&lt;:nombre:id&gt;</c>, <c>&lt;a:nombre:id&gt;</c> y <c>nombre:id</c>.
    /// </summary>
    public static CustomEmoji? ParseCustom(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        text = text.Trim();
        Match match = CustomEmojiRegex().Match(text);

        if (!match.Success || !ulong.TryParse(match.Groups[2].Value, NumberStyles.None, CultureInfo.InvariantCulture, out ulong id))
        {
            return null;
        }

        return new CustomEmoji(match.Groups[1].Value, id, text.StartsWith("<a:", StringComparison.Ordinal));
    }

    [GeneratedRegex(@"^<?a?:?([a-zA-Z0-9_]+):([0-9]+)>?$")]
    private static partial Regex CustomEmojiRegex();
}
