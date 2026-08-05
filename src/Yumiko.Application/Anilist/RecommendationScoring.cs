using Yumiko.Model.Entities;
using Yumiko.Model.Entities.Anilist;

namespace Yumiko.Application.Anilist;

public static class RecommendationScoring
{
    /// <summary>Minimum accumulated score for a recommendation to be shown.</summary>
    public const decimal MinimumScore = 3;

    /// <summary>
    /// Scores the recommendations hanging from the user list. Each scored entry contributes to its
    /// recommendations an adjusted score <c>(score - meanScore) / stdDev</c>, weighted by the rating
    /// of the node. The ones the user already has in this list or in the other type list are discarded.
    /// </summary>
    /// <param name="collection">User list for the queried type.</param>
    /// <param name="meanScore">Average user score for that type.</param>
    /// <param name="standardDeviation">User standard deviation. If it is 0 there is no way to normalize.</param>
    /// <param name="preferEnglishTitle">Whether the profile has titles configured in English.</param>
    /// <param name="excludedIds">Ids of the other type the user already watched or is watching.</param>
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
    /// Weight a node contributes based on its rating. The division is integer on purpose: it gives 1 with rating 1 and
    /// 2 with any higher rating. Touching it re-orders every recommendation.
    /// </summary>
    private static int RatingWeight(int nodeRating) => 2 - (1 / nodeRating);
}
