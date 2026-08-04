using Yumiko.Model.Enum;

namespace Yumiko.Application.Anilist;

public static class ScoreFormatter
{
    public static string FormatScore(decimal score, ScoreFormat format)
    {
        switch (format)
        {
            case ScoreFormat.POINT_10:
            case ScoreFormat.POINT_10_DECIMAL:
                return $"{score}/10";
            case ScoreFormat.POINT_100:
                return $"{score}/100";
            case ScoreFormat.POINT_5:
                string score5 = string.Empty;
                for (int i = 0; i < score; i++)
                {
                    score5 += "★";
                }

                return score5;
            case ScoreFormat.POINT_3:
                return score switch
                {
                    1 => "🙁",
                    2 => "😐",
                    3 => "🙂",
                    _ => throw new ArgumentOutOfRangeException(nameof(score)),
                };
            default:
                throw new ArgumentException("Invalid ScoreFormat type");
        }
    }

    // AniList devuelve el formato de puntaje del usuario como string crudo, sin normalizar al enum.
    public static string FormatScoreUser(string scoreFormat, string scorePers)
    {
        string scoreF = string.Empty;
        switch (scoreFormat)
        {
            case "POINT_10":
            case "POINT_10_DECIMAL":
                scoreF = $"{scorePers}/10";
                break;
            case "POINT_100":
                scoreF = $"{scorePers}/100";
                break;
            case "POINT_5":
                int scoreS = int.Parse(scorePers);
                for (int i = 0; i < scoreS; i++)
                {
                    scoreF += "★";
                }

                break;
            case "POINT_3":
                int score3 = int.Parse(scorePers);
                scoreF = score3 switch
                {
                    1 => "🙁",
                    2 => "😐",
                    3 => "🙂",
                    _ => scoreF,
                };

                break;
        }

        return scoreF;
    }
}
