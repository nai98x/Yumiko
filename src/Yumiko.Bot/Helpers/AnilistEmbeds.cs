using DSharpPlus;
using DSharpPlus.Entities;
using Yumiko.Application.Anilist;
using Yumiko.Application.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Model.Entities;
using Yumiko.Model.Entities.Anilist;
using Yumiko.Model.Enum;

namespace Yumiko.Bot.Helpers;

/// <summary>Embeds of the AniList commands.</summary>
public static class AnilistEmbeds
{
    public static DiscordEmbedBuilder Profile(User profile, Loc loc)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle(profile.Name)
            .WithThumbnail(profile.Avatar.Medium)
            .WithColor(YumikoColors.Primary);

        if (profile.BannerImage is not null)
        {
            embed.WithImageUrl(profile.BannerImage);
        }

        embed.AddField(loc[Keys.anime_stats],
            $"{loc[Keys.total]}: `{profile.Statistics.Anime.Count}`\n" +
            $"{loc[Keys.episodes]}: `{profile.Statistics.Anime.EpisodesWatched}`\n" +
            $"{loc[Keys.mean_score]}: `{profile.Statistics.Anime.MeanScore}`", true);

        embed.AddField(loc[Keys.manga_stats],
            $"{loc[Keys.total]}: `{profile.Statistics.Manga.Count}`\n" +
            $"{loc[Keys.chapters]}: `{profile.Statistics.Manga.ChaptersRead}`\n" +
            $"{loc[Keys.mean_score]}: `{profile.Statistics.Manga.MeanScore}`", true);

        embed.AddField(loc[Keys.settings],
            $"{loc[Keys.titles_language]}: `{profile.Options.TitleLanguage.UppercaseFirst()}`\n" +
            $"{loc[Keys.adult_content]}: {loc[profile.Options.DisplayAdultContent ? Keys.yes : Keys.no]}\n" +
            $"{loc[Keys.color]}: {profile.Options.ProfileColor}", true);

        bool english = profile.Options.TitleLanguage == "ENGLISH";

        AddFavourites(embed, $"📺 {loc[Keys.favorite_animes]}",
            profile.Favourites.Anime.Nodes?.Select(a => Formatter.MaskedUrl(PreferredTitle(a.Title, english), a.SiteUrl)));
        AddFavourites(embed, $"📖 {loc[Keys.favorite_mangas]}",
            profile.Favourites.Manga.Nodes?.Select(m => Formatter.MaskedUrl(PreferredTitle(m.Title, english), m.SiteUrl)));
        AddFavourites(embed, $"👤 {loc[Keys.favorite_characters]}",
            profile.Favourites.Characters.Nodes?.Select(c => Formatter.MaskedUrl(c.Name.Full, c.SiteUrl)));
        AddFavourites(embed, $"🧑‍🎨 {loc[Keys.favorite_staff]}",
            profile.Favourites.Staff.Nodes?.Select(s => Formatter.MaskedUrl(s.Name.Full, s.SiteUrl)));
        AddFavourites(embed, $"💽 {loc[Keys.favorite_studios]}",
            profile.Favourites.Studios.Nodes?.Select(s => Formatter.MaskedUrl(s.Name, s.SiteUrl)));

        return embed;
    }

    public static DiscordEmbedBuilder LoggedProfile(User profile, DiscordUser user, Loc loc)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Green)
            .WithTitle(loc[Keys.new_profile_saved])
            .WithDescription(loc.Format(Keys.new_profile_saved_mention, user.Mention))
            .WithThumbnail(profile.Avatar.Medium)
            .WithAuthor(profile.Name, profile.SiteUrl.AbsoluteUri, user.AvatarUrl);

        if (!string.IsNullOrEmpty(profile.BannerImage?.AbsoluteUri))
        {
            embed.WithImageUrl(profile.BannerImage.AbsoluteUri);
        }

        return embed;
    }

    public static DiscordEmbedBuilder Media(Media media, MediaType type, Loc loc)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle(media.Title.Romaji)
            .WithUrl(media.SiteUrl)
            .WithThumbnail(media.CoverImage.Large)
            .WithColor(YumikoColors.Primary);

        if (media.BannerImage is not null)
        {
            embed.WithImageUrl(media.BannerImage);
        }

        if (!string.IsNullOrEmpty(media.Description))
        {
            embed.WithDescription(TextHelper.CleanText(media.Description).NormalizeDescription());
        }

        if (type == MediaType.ANIME && media.Episodes is not null)
        {
            embed.AddField($"🔢 {loc[Keys.episodes]}", $"{media.Episodes}", true);
        }

        if (type == MediaType.MANGA && media.Chapters is not null)
        {
            embed.AddField($"🔢 {loc[Keys.chapters]}", $"{media.Chapters}", true);
        }

        if (media.Format is not null)
        {
            embed.AddField($"🗂️ {loc[Keys.format]}", $"{media.Format}".UppercaseFirst(), true);
        }

        if (media.Status is not null)
        {
            embed.AddField($"⏳ {loc[Keys.status]}", $"{media.Status}".UppercaseFirst(), true);
        }

        if (media.MeanScore is not null)
        {
            embed.AddField($"⭐ {loc[Keys.score]}", $"{media.MeanScore}");
        }

        string dates = string.Empty;
        if (media.StartDate.Day is not null)
        {
            dates += $"{Formatter.Bold(loc[Keys.from])}: {media.StartDate.Day}/{media.StartDate.Month}/{media.StartDate.Year}\n";
        }

        if (media.EndDate.Day is not null)
        {
            dates += $"{Formatter.Bold(loc[Keys.to])}: {media.EndDate.Day}/{media.EndDate.Month}/{media.EndDate.Year}";
        }

        AddIfPresent(embed, $"🗓️ {loc[Keys.start_date]}", dates);
        AddIfPresent(embed, $"📜 {loc[Keys.genres]}", Join(media.Genres));
        AddIfPresent(embed, $"🗒️ {loc[Keys.tags]}", Join(media.Tags?.Select(t => t.IsMediaSpoiler ? $"||{t.Name}||" : t.Name)));
        AddIfPresent(embed, $"✏️ {loc[Keys.synonyms]}", media.Synonyms?.Count > 0 ? Join(media.Synonyms) : loc[Keys.without_titles]);
        AddIfPresent(embed, $"💽 {loc[Keys.studios]}", Join(media.Studios?.Nodes?.Select(s => Formatter.MaskedUrl(s.Name, s.SiteUrl))));
        AddIfPresent(embed, $"🔗 {loc[Keys.external_links]}", Join(media.ExternalLinks?.Select(l => Formatter.MaskedUrl(l.Site, l.Url))));

        return embed;
    }

    public static DiscordEmbedBuilder MediaUserStats(MediaUserStatistics mediaUser, Loc loc)
    {
        string progress = $"{mediaUser.Progress}";
        if (mediaUser.Media.Episodes is not null)
        {
            progress += $"/{mediaUser.Media.Episodes}";
        }

        if (mediaUser.Media.Chapters is not null)
        {
            progress += $"/{mediaUser.Media.Chapters}";
        }

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle($"{loc[Keys.stats]}: {mediaUser.User.Name}")
            .WithDescription($"{Formatter.Bold(loc[Keys.notes])}\n{(string.IsNullOrEmpty(mediaUser.Notes) ? loc[Keys.without_notes] : mediaUser.Notes)}".NormalizeDescription())
            .WithColor(YumikoColors.Primary)
            .WithThumbnail(mediaUser.User.Avatar.Medium)
            .WithUrl(mediaUser.User.SiteUrl)
            .AddField(loc[Keys.status], $"{mediaUser.Status}".UppercaseFirst(), true)
            .AddField(mediaUser.Media.Episodes is not null ? loc[Keys.episodes] : loc[Keys.chapters], progress, true)
            .AddField(loc[Keys.score], mediaUser.Score > 0
                ? ScoreFormatter.FormatScore(mediaUser.Score, mediaUser.User.MediaListOptions.ScoreFormat)
                : loc[Keys.not_assigned], true)
            .AddField("Rewatches", $"{mediaUser.Repeat}");

        if (mediaUser.StartedAt.Day is not null)
        {
            embed.AddField(loc[Keys.start_date], $"{mediaUser.StartedAt.Day}/{mediaUser.StartedAt.Month}/{mediaUser.StartedAt.Year}", true);
        }

        if (mediaUser.CompletedAt.Day is not null)
        {
            embed.AddField(loc[Keys.end_date], $"{mediaUser.CompletedAt.Day}/{mediaUser.CompletedAt.Month}/{mediaUser.CompletedAt.Year}", true);
        }

        return embed;
    }

    public static DiscordEmbedBuilder Character(Character character)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle(character.Name.Full)
            .WithUrl(character.SiteUrl)
            .WithThumbnail(character.Image.Large)
            .WithColor(YumikoColors.Primary);

        if (!string.IsNullOrEmpty(character.Description))
        {
            embed.WithDescription(TextHelper.CleanText(character.Description).NormalizeDescription());
        }

        AddIfPresent(embed, "📺 Animes", Join(character.Animes.Nodes?.Select(a => Formatter.MaskedUrl(a.Title.Romaji, a.SiteUrl)), "\n"));
        AddIfPresent(embed, "📖 Mangas", Join(character.Mangas.Nodes?.Select(m => Formatter.MaskedUrl(m.Title.Romaji, m.SiteUrl)), "\n"));

        return embed;
    }

    public static DiscordEmbedBuilder RandomCharacter(Character character, int page, Loc loc)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle(character.Name.Full)
            .WithUrl(character.SiteUrl)
            .WithImageUrl(character.Image.Large)
            .WithColor(YumikoColors.Primary)
            .WithFooter($"{character.Favourites} ❤️ (nº {page} {loc[Keys.in_popularity_rank]})", YumikoColors.AnilistAvatarUrl);

        if (character.Animes?.Nodes?.Count > 0)
        {
            embed.WithDescription($"{$"{MediaType.ANIME}".UppercaseFirst()}: {Formatter.MaskedUrl(character.Animes.Nodes[0].Title.Romaji, character.Animes.Nodes[0].SiteUrl)}\n");
        }
        else if (character.Mangas?.Nodes?.Count > 0)
        {
            embed.WithDescription($"{$"{MediaType.MANGA}".UppercaseFirst()}: {Formatter.MaskedUrl(character.Mangas.Nodes[0].Title.Romaji, character.Mangas.Nodes[0].SiteUrl)}\n");
        }

        return embed;
    }

    public static DiscordEmbedBuilder Staff(Staff staff, Loc loc)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithColor(YumikoColors.Primary)
            .WithTitle(staff.Name.Full)
            .WithThumbnail(staff.Image.Large);

        if (staff.Description is not null)
        {
            embed.WithDescription(TextHelper.CleanText(staff.Description).NormalizeDescription());
        }

        if (staff.LanguageV2 is not null)
        {
            embed.AddField(loc[Keys.language], staff.LanguageV2);
        }

        if (staff.Age is not null)
        {
            embed.AddField(loc[Keys.age], $"{staff.Age}");
        }

        if (staff.Gender is not null)
        {
            embed.AddField(loc[Keys.gender], staff.Gender);
        }

        if (staff.DateOfBirth.Day is not null)
        {
            embed.AddField(loc[Keys.date_of_birth], $"{staff.DateOfBirth.Day}/{staff.DateOfBirth.Month}/{staff.DateOfBirth.Year}", true);
        }

        if (staff.DateOfDeath.Day is not null)
        {
            embed.AddField(loc[Keys.date_of_death], $"{staff.DateOfDeath.Day}/{staff.DateOfDeath.Month}/{staff.DateOfDeath.Year}", true);
        }

        return embed;
    }

    public static DiscordEmbedBuilder Recommendations(
        IReadOnlyList<AnimeRecommendation> recommendations,
        User profile,
        DiscordUser user,
        MediaType type,
        Loc loc)
    {
        if (recommendations.Count == 0)
        {
            return new DiscordEmbedBuilder()
                .WithTitle(loc[Keys.error])
                .WithDescription(loc[Keys.no_recommendations_found])
                .WithColor(DiscordColor.Red)
                .WithAuthor(profile.Name, profile.SiteUrl.AbsoluteUri, profile.Avatar.Medium.AbsoluteUri)
                .WithThumbnail(user.AvatarUrl);
        }

        string typeName = $"{type}".ToLowerInvariant();
        string description = string.Join("\n", recommendations.Select(rec =>
            $"{Formatter.Bold($"{rec.Score:##.##}")} - {Formatter.MaskedUrl(rec.Title, new Uri($"https://anilist.co/{typeName}/{rec.Id}"))}"));

        return new DiscordEmbedBuilder()
            .WithTitle(loc.Format(Keys.media_recommendations, $"{type}".UppercaseFirst()))
            .WithDescription(description.NormalizeDescriptionNewLine())
            .WithColor(YumikoColors.Primary)
            .WithFooter(
                loc.Format(Keys.media_recommendations_explanation, loc[type == MediaType.ANIME ? Keys.watched : Keys.read], typeName),
                YumikoColors.AnilistAvatarUrl)
            .WithAuthor(profile.Name, profile.SiteUrl.AbsoluteUri, profile.Avatar.Medium.AbsoluteUri)
            .WithThumbnail(user.AvatarUrl);
    }

    private static string PreferredTitle(MediaTitle title, bool english) =>
        english && !string.IsNullOrEmpty(title.English) ? title.English : title.Romaji;

    private static string Join(IEnumerable<string>? values, string separator = ", ") =>
        values is null ? string.Empty : string.Join(separator, values);

    private static void AddIfPresent(DiscordEmbedBuilder embed, string title, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            embed.AddField(title, value.NormalizeField());
        }
    }

    private static void AddFavourites(DiscordEmbedBuilder embed, string title, IEnumerable<string>? values)
    {
        string content = Join(values, "\n");
        if (!string.IsNullOrEmpty(content))
        {
            embed.AddField(title, content.NormalizeField(), true);
        }
    }
}
