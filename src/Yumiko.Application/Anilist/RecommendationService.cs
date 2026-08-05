using Yumiko.Model.Entities;
using Yumiko.Model.Entities.Anilist;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces;

namespace Yumiko.Application.Anilist;

/// <summary>
/// Builds the recommendations of a user: fetches their list of the requested type, the one of the other
/// type (to exclude what they already watched) and scores it with <see cref="RecommendationScoring"/>.
/// </summary>
public sealed class RecommendationService(IAnilistClient anilist)
{
    public async Task<(User? Profile, List<AnimeRecommendation> Recommendations)> GetAsync(
        int anilistUserId,
        MediaType type,
        CancellationToken cancellationToken = default)
    {
        (User? profile, MediaListCollection? collection) = await anilist.GetRecommendationsAsync(anilistUserId, type, cancellationToken);

        if (profile is null || collection is null)
        {
            return (profile, []);
        }

        MediaType otherType = type == MediaType.ANIME ? MediaType.MANGA : MediaType.ANIME;

        MediaUserList? completed = await anilist.GetMediaListsAsync(
            profile.Id, MediaUserStatus.COMPLETED, MediaUserSort.MEDIA_POPULARITY_DESC, MediaTitleType.ROMAJI, otherType, cancellationToken);
        MediaUserList? inProgress = await anilist.GetMediaListsAsync(
            profile.Id, MediaUserStatus.CURRENT, MediaUserSort.MEDIA_POPULARITY_DESC, MediaTitleType.ROMAJI, otherType, cancellationToken);

        HashSet<int> excludedIds =
        [
            .. (completed?.Entries ?? []).Select(e => e.Media.Id),
            .. (inProgress?.Entries ?? []).Select(e => e.Media.Id),
        ];

        // Anime and manga statistics are different types in the model, so they are not unified.
        decimal meanScore = type == MediaType.ANIME ? profile.Statistics.Anime.MeanScore : profile.Statistics.Manga.MeanScore;
        decimal deviation = type == MediaType.ANIME ? profile.Statistics.Anime.StandardDeviation : profile.Statistics.Manga.StandardDeviation;

        List<AnimeRecommendation> recommendations = RecommendationScoring.Score(
            collection,
            meanScore,
            deviation,
            profile.Options.TitleLanguage == "ENGLISH",
            excludedIds);

        return (profile, recommendations);
    }
}
