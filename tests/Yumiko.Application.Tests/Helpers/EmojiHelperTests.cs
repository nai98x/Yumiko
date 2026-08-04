using Yumiko.Application.Helpers;

namespace Yumiko.Application.Tests.Helpers;

public class EmojiHelperTests
{
    [Theory]
    [InlineData("<:yumiko:850000000000000000>", "yumiko", 850000000000000000ul, false)]
    [InlineData("<a:baila:850000000000000001>", "baila", 850000000000000001ul, true)]
    [InlineData("yumiko:850000000000000000", "yumiko", 850000000000000000ul, false)]
    [InlineData("  <:yumiko:850000000000000000>  ", "yumiko", 850000000000000000ul, false)]
    public void ParseCustom_RecognizesTheThreeForms(string text, string name, ulong id, bool animated)
    {
        CustomEmoji? emoji = EmojiHelper.ParseCustom(text);

        Assert.NotNull(emoji);
        Assert.Equal(name, emoji.Name);
        Assert.Equal(id, emoji.Id);
        Assert.Equal(animated, emoji.Animated);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("😀")]
    [InlineData("yumiko")]
    [InlineData("<:yumi ko:850000000000000000>")]
    public void ParseCustom_ReturnsNullWhenNotACustomEmote(string? text)
    {
        Assert.Null(EmojiHelper.ParseCustom(text));
    }

    [Fact]
    public void CustomEmoji_DerivesUrlAndDateFromSnowflake()
    {
        CustomEmoji estatico = new("yumiko", 850000000000000000ul, false);
        CustomEmoji animated = new("baila", 850000000000000000ul, true);

        Assert.Equal("https://cdn.discordapp.com/emojis/850000000000000000.png", estatico.Url);
        Assert.Equal("https://cdn.discordapp.com/emojis/850000000000000000.gif", animated.Url);
        Assert.Equal(2021, estatico.CreationTimestamp.Year);
    }
}
