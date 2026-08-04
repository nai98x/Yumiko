using System.Diagnostics;
using System.Globalization;
using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using Yumiko.Application.Games;
using Yumiko.Application.Helpers;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services.State;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Bot.Games;

public sealed class HigherOrLowerGameRunner(
    AnilistMediaCacheState mediaCache,
    IHigherOrLowerLeaderboardRepository leaderboard,
    IHttpClientFactory httpClientFactory)
{
    private const string CancelCustomId = "hol-cancel";
    private const int ImageWidth = 500;
    private const int ImageHeight = 375;

    private static readonly TimeSpan AnswerTimeout = TimeSpan.FromSeconds(15);

    /// <summary>
    /// El token de una interacción vence a los 15 minutos; se corta un minuto antes para poder mandar
    /// el mensaje de cierre.
    /// </summary>
    private static readonly TimeSpan MaxDuration = TimeSpan.FromMinutes(14);

    public async Task PlayAsync(SlashCommandContext ctx, GamemodeHoL gamemode, InteractivityExtension interactivity, Loc loc)
    {
        Stopwatch clock = Stopwatch.StartNew();
        List<Anime> list = [.. mediaCache.Media];

        DiscordEmbed? previousEmbed = null;
        int score = 0;

        while (list.Count >= 2)
        {
            if (HigherOrLower.PickPair(list) is not { } pair)
            {
                break;
            }

            (Anime first, Anime second) = pair;

            DiscordMessage message = await ctx.FollowupAsync(await BuildRoundAsync(first, second, gamemode, previousEmbed, loc));

            InteractivityResult<ComponentInteractionCreatedEventArgs> answer =
                await interactivity.WaitForButtonAsync(message, ctx.User, AnswerTimeout);

            if (answer.TimedOut)
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = loc[Keys.defeat],
                    Description = $"{loc.Format(Keys.no_answser_in_time, AnswerTimeout.TotalSeconds)}\n\n{loc[Keys.score]}: **{score}**",
                    Color = DiscordColor.Red,
                }));
                break;
            }

            if (answer.Result.Id == CancelCustomId)
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder { Title = loc[Keys.game_cancelled], Color = DiscordColor.Red }));
                break;
            }

            bool choseFirst = answer.Result.Id == $"{first.Id}";
            Anime selected = choseFirst ? first : second;
            Anime other = choseFirst ? second : first;
            TimeSpan elapsed = answer.Result.Interaction.CreationTimestamp - message.CreationTimestamp;

            if (!HigherOrLower.IsCorrect(selected, other, gamemode))
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = loc.Format(Keys.miss_user_games, DisplayNameOf(ctx)),
                    Description = $"**{loc[Keys.defeat]}**\n\n{Comparison(selected, other, gamemode, false, loc)}\n\n{loc[Keys.score]}: **{score}**",
                    Color = DiscordColor.Red,
                    Footer = Footer(ctx, elapsed, loc),
                }));
                break;
            }

            score++;
            previousEmbed = new DiscordEmbedBuilder
            {
                Title = loc.Format(Keys.guess_user, DisplayNameOf(ctx)),
                Description = $"{Comparison(selected, other, gamemode, true, loc)}\n\n{loc[Keys.score]}: **{score}**",
                Color = DiscordColor.Green,
                Footer = Footer(ctx, elapsed, loc),
            };

            list.Remove(first);
            list.Remove(second);

            if (list.Count < 2 || clock.Elapsed >= MaxDuration)
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder { Title = loc[Keys.victory], Color = DiscordColor.Green }));
                break;
            }
        }

        clock.Stop();

        if (score > 0 && await leaderboard.AddResultAsync(ctx.Guild!.Id, ctx.User.Id, score))
        {
            await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = loc[Keys.new_record],
                Description = $"{loc.Format(Keys.new_record_desc, ctx.User.Mention)}\n\n{loc.Format(Keys.your_new_record_is, score)}",
                Color = YumikoColors.Primary,
            }));
        }
    }

    private async Task<DiscordFollowupMessageBuilder> BuildRoundAsync(
        Anime first,
        Anime second,
        GamemodeHoL gamemode,
        DiscordEmbed? previousEmbed,
        Loc loc)
    {
        DiscordFollowupMessageBuilder builder = new DiscordFollowupMessageBuilder()
            .AddActionRowComponent(
                new DiscordButtonComponent(DiscordButtonStyle.Primary, $"{first.Id}", first.TitleRomaji.NormalizeButton()),
                new DiscordButtonComponent(DiscordButtonStyle.Primary, $"{second.Id}", second.TitleRomaji.NormalizeButton()),
                new DiscordButtonComponent(DiscordButtonStyle.Danger, CancelCustomId, loc[Keys.finish_game]));

        if (await BuildImageAsync(first, second) is { } image)
        {
            builder.AddFile("image.png", image.ToMemoryStream());
        }

        if (previousEmbed is not null)
        {
            builder.AddEmbed(previousEmbed);
        }

        return builder.AddEmbed(new DiscordEmbedBuilder
        {
            Title = loc[gamemode == GamemodeHoL.Score ? Keys.which_one_has_better_score : Keys.which_one_is_more_popular],
            Color = YumikoColors.Primary,
            ImageUrl = "attachment://image.png",
        });
    }

    private async Task<byte[]?> BuildImageAsync(Anime first, Anime second)
    {
        if (first.Image is null || second.Image is null)
        {
            return null;
        }

        HttpClient client = httpClientFactory.CreateClient();
        byte[] bytes1 = await client.GetByteArrayAsync(first.Image);
        byte[] bytes2 = await client.GetByteArrayAsync(second.Image);

        byte[] merged = ImageHelper.MergeImage(bytes1, bytes2, ImageWidth, ImageHeight);
        byte[] frame = await File.ReadAllBytesAsync(Path.Join(AppContext.BaseDirectory, "Images", "frame-hol.png"));

        return ImageHelper.OverlapImage(merged, frame, ImageWidth, ImageHeight);
    }

    private static string Comparison(Anime selected, Anime other, GamemodeHoL gamemode, bool isCorrect, Loc loc)
    {
        if (gamemode == GamemodeHoL.Popularity)
        {
            return loc.Format(
                isCorrect ? Keys.higher_or_lower_round_win_popularity : Keys.higher_or_lower_round_defeat_popularity,
                selected.TitleRomaji, selected.Favourites, other.TitleRomaji, other.Favourites);
        }

        return loc.Format(
            isCorrect ? Keys.higher_or_lower_round_win : Keys.higher_or_lower_round_defeat,
            selected.TitleRomaji, HigherOrLower.ScoreOutOfTen(selected),
            other.TitleRomaji, HigherOrLower.ScoreOutOfTen(other));
    }

    private static DiscordEmbedBuilder.EmbedFooter Footer(SlashCommandContext ctx, TimeSpan elapsed, Loc loc) => new()
    {
        IconUrl = ctx.User.AvatarUrl,
        Text = $"{loc[Keys.time]}: {elapsed.TotalSeconds.ToString("0.##", CultureInfo.InvariantCulture)}s",
    };

    private static string DisplayNameOf(SlashCommandContext ctx) =>
        ctx.Member?.DisplayName ?? ctx.User.Username;
}
