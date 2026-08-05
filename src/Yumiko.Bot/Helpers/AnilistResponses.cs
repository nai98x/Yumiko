using DSharpPlus.Entities;
using Yumiko.Application.Anilist;
using Yumiko.Bot.Localization;
using Yumiko.Model.Entities;
using Yumiko.Model.Entities.Anilist;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Bot.Helpers;

/// <summary>Responses shared by the <c>/anilist</c> slash commands and the context menus.</summary>
public sealed class AnilistResponses(
    IAnilistClient anilist,
    IAnilistUsersRepository anilistUsers,
    RecommendationService recommendationService)
{
    public async Task<DiscordWebhookBuilder> ProfileAsync(DiscordUser user, Loc loc)
    {
        User? profile = await FindProfileAsync(user.Id);

        if (profile is null)
        {
            return new DiscordWebhookBuilder().AddEmbed(ErrorEmbed.Create(
                loc[Keys.anilist_profile_not_found],
                $"{loc.Format(Keys.no_anilist_profile_vinculated, user.Mention)}.\n\n" +
                $"{loc[Keys.to_vinculate_anilist_profile]}: `/anilist setprofile`"));
        }

        return new DiscordWebhookBuilder()
            .AddEmbed(AnilistEmbeds.Profile(profile, loc))
            .AddActionRowComponent(
                new DiscordLinkButtonComponent($"{profile.SiteUrl}", loc[Keys.profile], false, new DiscordComponentEmoji("👤")),
                new DiscordLinkButtonComponent($"{profile.SiteUrl}/animelist", loc[Keys.anime_list], false, new DiscordComponentEmoji("📺")),
                new DiscordLinkButtonComponent($"{profile.SiteUrl}/mangalist", loc[Keys.manga_list], false, new DiscordComponentEmoji("📖")));
    }

    public async Task<DiscordEmbedBuilder> RecommendationsAsync(DiscordUser user, MediaType type, Loc loc)
    {
        AnilistUserLink? link = await anilistUsers.GetLinkAsync(user.Id);

        if (link is null)
        {
            return ErrorEmbed.Create(loc[Keys.error], loc[Keys.anilist_profile_not_found]);
        }

        (User? profile, List<AnimeRecommendation> recommendations) = await recommendationService.GetAsync(link.AnilistId, type);

        return profile is null
            ? ErrorEmbed.Unknown(loc)
            : AnilistEmbeds.Recommendations(recommendations, profile, user, type, loc);
    }

    public async Task<User?> FindProfileAsync(ulong userId)
    {
        AnilistUserLink? link = await anilistUsers.GetLinkAsync(userId);
        return link is null ? null : await anilist.GetProfileAsync(link.AnilistId);
    }
}
