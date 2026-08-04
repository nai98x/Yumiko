using Yumiko.Application.Anilist;
using Yumiko.Model.Enum;

namespace Yumiko.Application.Tests.Anilist;

public class ScoreFormatterTests
{
    [Theory]
    [InlineData(ScoreFormat.POINT_10, 8, "8/10")]
    [InlineData(ScoreFormat.POINT_10_DECIMAL, 8, "8/10")]
    [InlineData(ScoreFormat.POINT_100, 85, "85/100")]
    public void FormatScore_NumericFormats(ScoreFormat format, int score, string expected)
    {
        Assert.Equal(expected, ScoreFormatter.FormatScore(score, format));
    }

    [Fact]
    public void FormatScore_Point5_ReturnsStars()
    {
        Assert.Equal("★★★", ScoreFormatter.FormatScore(3, ScoreFormat.POINT_5));
        Assert.Equal(string.Empty, ScoreFormatter.FormatScore(0, ScoreFormat.POINT_5));
    }

    [Theory]
    [InlineData(1, "🙁")]
    [InlineData(2, "😐")]
    [InlineData(3, "🙂")]
    public void FormatScore_Point3_ReturnsASmiley(int score, string expected)
    {
        Assert.Equal(expected, ScoreFormatter.FormatScore(score, ScoreFormat.POINT_3));
    }

    [Fact]
    public void FormatScore_Point3_OutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ScoreFormatter.FormatScore(4, ScoreFormat.POINT_3));
    }

    [Theory]
    [InlineData("POINT_10", "8", "8/10")]
    [InlineData("POINT_10_DECIMAL", "8.5", "8.5/10")]
    [InlineData("POINT_100", "85", "85/100")]
    [InlineData("POINT_5", "3", "★★★")]
    [InlineData("POINT_3", "1", "🙁")]
    [InlineData("POINT_3", "2", "😐")]
    [InlineData("POINT_3", "3", "🙂")]
    public void FormatScoreUser_RawAnilistFormats(string format, string score, string expected)
    {
        Assert.Equal(expected, ScoreFormatter.FormatScoreUser(format, score));
    }

    [Fact]
    public void FormatScoreUser_UnknownFormat_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, ScoreFormatter.FormatScoreUser("POINT_7", "5"));
    }

    [Fact]
    public void FormatScoreUser_Point3_OutOfRange_ReturnsEmpty()
    {
        // A diferencia de FormatScore, la variante de string no tira: devuelve vacío.
        Assert.Equal(string.Empty, ScoreFormatter.FormatScoreUser("POINT_3", "4"));
    }

    [Fact]
    public void FormatScore_InvalidFormat_Throws()
    {
        Assert.Throws<ArgumentException>(() => ScoreFormatter.FormatScore(5, (ScoreFormat)99));
    }
}
