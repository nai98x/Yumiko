using Yumiko.Model.Entities;
using Yumiko.Model.Entities.Anilist;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces;

namespace Yumiko.Application.Anilist;

/// <summary>
/// Arma las recomendaciones de un usuario: trae su lista del tipo pedido, la del otro tipo (para
/// excluir lo que ya vio) y puntúa con <see cref="RecommendationScoring"/>.
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

        // Las estadísticas de anime y manga son tipos distintos en el modelo, así que no se unifican.
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
