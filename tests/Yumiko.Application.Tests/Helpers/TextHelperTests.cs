using Yumiko.Application.Helpers;

namespace Yumiko.Application.Tests.Helpers;

public class TextHelperTests
{
    [Fact]
    public void CleanText_NullOrEmpty_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, TextHelper.CleanText(null));
        Assert.Equal(string.Empty, TextHelper.CleanText(string.Empty));
    }

    [Theory]
    [InlineData("<br>", "")]
    [InlineData("<Br>", "")]
    [InlineData("<bR>", "")]
    [InlineData("<BR>", "")]
    public void CleanText_StripsHtmlLineBreaks_InAnyCasing(string entry, string expected)
    {
        Assert.Equal(expected, TextHelper.CleanText(entry));
    }

    [Theory]
    [InlineData("<i>Shingeki no Kyojin</i>", "*Shingeki no Kyojin*")]
    [InlineData("<I>Shingeki no Kyojin</I>", "*Shingeki no Kyojin*")]
    [InlineData("<b>Eren</b>", "**Eren**")]
    [InlineData("<B>Eren</B>", "**Eren**")]
    [InlineData("__Eren__", "**Eren**")]
    public void CleanText_TranslatesEmphasisToDiscordMarkdown(string entry, string expected)
    {
        Assert.Equal(expected, TextHelper.CleanText(entry));
    }

    [Fact]
    public void CleanText_TranslatesAnilistSpoilersToDiscordSpoilers()
    {
        Assert.Equal("Al final ||muere Sasha||.", TextHelper.CleanText("Al final ~!muere Sasha!~."));
    }

    [Fact]
    public void CleanText_RealAnilistDescription()
    {
        const string description =
            "Several hundred years ago, humans were nearly exterminated by titans.<br>\n" +
            "<br>\n" +
            "<i>Note: The anime is based on the manga by <b>Hajime Isayama</b>.</i><br>\n" +
            "~!Spoiler: the walls are made of titans.!~";

        const string expected =
            "Several hundred years ago, humans were nearly exterminated by titans.\n" +
            "\n" +
            "*Note: The anime is based on the manga by **Hajime Isayama**.*\n" +
            "||Spoiler: the walls are made of titans.||";

        Assert.Equal(expected, TextHelper.CleanText(description));
    }

    [Fact]
    public void CleanText_LeavesUnlistedTagsAlone()
    {
        // Only the exact variants AniList returns are replaced; `<br />` and `<em>` survive.
        Assert.Equal("<br /><em>x</em>", TextHelper.CleanText("<br /><em>x</em>"));
    }

    [Fact]
    public void RemoveSpecialCharacters_NullOrEmpty_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, TextHelper.RemoveSpecialCharacters(null));
        Assert.Equal(string.Empty, TextHelper.RemoveSpecialCharacters(string.Empty));
    }

    [Theory]
    [InlineData("Fullmetal Alchemist: Brotherhood", "Fullmetal Alchemist Brotherhood")]
    [InlineData("Re:Zero kara Hajimeru Isekai Seikatsu", "Re Zero kara Hajimeru Isekai Seikatsu")]
    [InlineData("  ¡Kimi no Na wa.!  ", "Kimi no Na wa")]
    [InlineData("K-On!!", "K On")]
    public void RemoveSpecialCharacters_CollapsesNonAlphanumericsIntoOneSpaceAndTrims(string entry, string expected)
    {
        Assert.Equal(expected, TextHelper.RemoveSpecialCharacters(entry));
    }

    [Fact]
    public void RemoveSpecialCharacters_StripsAccents()
    {
        // The regex is pure ASCII: accented vowels count as separators, not as letters.
        Assert.Equal("Bakemonogatari", TextHelper.RemoveSpecialCharacters("Bakemonogatari"));
        Assert.Equal("Pok mon", TextHelper.RemoveSpecialCharacters("Pokémon"));
    }

    [Theory]
    [InlineData("cielo claro", "Cielo Claro")]
    [InlineData("BROKEN CLOUDS", "Broken Clouds")]
    [InlineData("light rain", "Light Rain")]
    [InlineData("", "")]
    public void UppercaseFirst_TitleCasesFromLowercase(string entry, string expected)
    {
        Assert.Equal(expected, entry.UppercaseFirst());
    }

    [Fact]
    public void UppercaseFirst_NullReturnsEmpty()
    {
        Assert.Equal(string.Empty, TextHelper.UppercaseFirst(null));
    }
}
