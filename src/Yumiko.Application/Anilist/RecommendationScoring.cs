using Yumiko.Model.Entities;
using Yumiko.Model.Entities.Anilist;

namespace Yumiko.Application.Anilist;

public static class RecommendationScoring
{
    /// <summary>Puntaje mínimo acumulado para que una recomendación se muestre.</summary>
    public const decimal MinimumScore = 3;

    /// <summary>
    /// Puntúa las recomendaciones que cuelgan de la lista del usuario. Cada entrada puntuada aporta a sus
    /// recomendaciones un puntaje ajustado <c>(score - meanScore) / stdDev</c>, ponderado por el rating
    /// del nodo. Se descartan las que el usuario ya tiene en esta lista o en la del otro tipo.
    /// </summary>
    /// <param name="collection">Lista del usuario para el tipo consultado.</param>
    /// <param name="meanScore">Puntaje promedio del usuario para ese tipo.</param>
    /// <param name="standardDeviation">Desvío estándar del usuario. Si es 0 no se puede normalizar.</param>
    /// <param name="preferEnglishTitle">Si el perfil tiene los títulos configurados en inglés.</param>
    /// <param name="excludedIds">Ids del otro tipo que el usuario ya vio o está viendo.</param>
    public static List<AnimeRecommendation> Score(
        MediaListCollection? collection,
        decimal meanScore,
        decimal standardDeviation,
        bool preferEnglishTitle,
        HashSet<int> excludedIds)
    {
        List<AnimeRecommendation> recommendations = [];

        if (standardDeviation == 0 || collection?.Lists is null)
        {
            return recommendations;
        }

        HashSet<int> alreadyInList = [];
        foreach (MediaList list in collection.Lists)
        {
            if (list.Entries is null)
            {
                continue;
            }

            foreach (MediaEntry entry in list.Entries)
            {
                alreadyInList.Add(entry.MediaId);
            }
        }

        Dictionary<int, AnimeRecommendation> byId = [];

        foreach (MediaList list in collection.Lists)
        {
            foreach (MediaEntry entry in list.Entries ?? [])
            {
                if (entry.Score is not > 0 || entry.Media?.Recommendations?.Nodes is null)
                {
                    continue;
                }

                decimal adjustedScore = ((decimal)entry.Score - meanScore) / standardDeviation;

                foreach (RecommendationNode node in entry.Media.Recommendations.Nodes)
                {
                    if (node.MediaRecommendation is null)
                    {
                        continue;
                    }

                    int nodeId = node.MediaRecommendation.Id;
                    int nodeRating = node.Rating;

                    if (nodeRating <= 0 || alreadyInList.Contains(nodeId) || excludedIds.Contains(nodeId))
                    {
                        continue;
                    }

                    if (!byId.TryGetValue(nodeId, out AnimeRecommendation? recommendation))
                    {
                        recommendation = new AnimeRecommendation
                        {
                            Id = nodeId,
                            Title = preferEnglishTitle && node.MediaRecommendation.Title.English != null
                                ? node.MediaRecommendation.Title.English
                                : node.MediaRecommendation.Title.Romaji,
                        };
                        byId[nodeId] = recommendation;
                        recommendations.Add(recommendation);
                    }

                    recommendation.Score += adjustedScore * RatingWeight(nodeRating);
                }
            }
        }

        return [.. recommendations.OrderByDescending(x => x.Score).Where(x => x.Score >= MinimumScore)];
    }

    /// <summary>
    /// Peso que aporta un nodo según su rating. La división es entera a propósito: da 1 con rating 1 y
    /// 2 con cualquier rating mayor. Tocarla re-ordena todas las recomendaciones.
    /// </summary>
    private static int RatingWeight(int nodeRating) => 2 - (1 / nodeRating);
}
