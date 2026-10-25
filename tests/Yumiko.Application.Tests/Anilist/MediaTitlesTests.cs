using Yumiko.Application.Anilist;

namespace Yumiko.Application.Tests.Anilist;

public class MediaTitlesTests
{
    [Theory]
    [InlineData("Shingeki no Kyojin", "Shingeki no Kyojin Season 2")]
    [InlineData("Shingeki no Kyojin Season 2", "Shingeki no Kyojin")]
    [InlineData("Shingeki no Kyojin", "Shingeki no Kyojin: The Final Season")]
    [InlineData("Hagane no Renkinjutsushi: FULLMETAL ALCHEMIST", "Hagane no Renkinjutsushi")]
    [InlineData("Gintama", "Gintama°")]
    [InlineData("ONE PIECE", "one piece")]
    [InlineData("  Kimetsu no Yaiba ", "Kimetsu no Yaiba: Yuukaku-hen")]
    [InlineData("Shingeki no Kyojin Season 3 Part 2", "Shingeki no Kyojin Season 2")]
    [InlineData("Mushoku Tensei: Isekai Ittara Honki Dasu", "Mushoku Tensei II: Isekai Ittara Honki Dasu")]
    [InlineData("Haikyuu!!", "Haikyuu!! Second Season")]
    public void AreSameSeries_WhenOneTitleExtendsTheOther(string first, string second) =>
        Assert.True(MediaTitles.AreSameSeries(first, second));

    [Theory]
    [InlineData("ONE PIECE", "Dragon Ball Z")]
    [InlineData("K", "Kanon")]
    [InlineData("Boku no Hero Academia", "Boku no Pico")]
    [InlineData("Gin no Saji", "Gintama")]
    [InlineData("Kimi no Na wa.", "Kimi no Koe wo Todoketai")]
    [InlineData("Tensei shitara Slime Datta Ken", "Tensei Kizoku no Isekai Boukenroku")]
    [InlineData("", "ONE PIECE")]
    [InlineData(null, "ONE PIECE")]
    [InlineData(null, null)]
    public void AreSameSeries_WhenTitlesAreUnrelated(string? first, string? second) =>
        Assert.False(MediaTitles.AreSameSeries(first, second));

    [Theory]
    [InlineData("Violet Evergarden", "Violet Evergarden")]
    [InlineData("Sousou no Frieren", "Frieren")]
    [InlineData("Code Geass: Hangyaku no Lelouch", "Lelouch Lamperouge")]
    [InlineData("Horimiya", "Kyouko Hori")]
    [InlineData("Mahou Shoujo Madoka☆Magica", "Madoka Kaname")]
    [InlineData("NARUTO", "Naruto Uzumaki")]
    public void ContainsName_WhenTheNameGivesTheTitleAway(string title, string name) =>
        Assert.True(MediaTitles.ContainsName(title, name));

    [Theory]
    [InlineData("Shingeki no Kyojin", "Eren Yeager")]
    [InlineData("DEATH NOTE", "Light Yagami")]
    [InlineData("Lucky☆Star", "Konata Izumi")]
    [InlineData("Ai no Kusabi", "Iason Mink")]
    [InlineData("ONE PIECE", "")]
    [InlineData(null, "Luffy")]
    public void ContainsName_WhenTheNameIsUnrelated(string? title, string? name) =>
        Assert.False(MediaTitles.ContainsName(title, name));
}
