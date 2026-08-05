using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Yumiko.Application.Games;
using Yumiko.Application.Helpers;
using Yumiko.Bot.Commands.Framework;
using Yumiko.Bot.Commands.Framework.Attributes;
using Yumiko.Bot.Commands.Framework.Choices;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Extensions;
using Yumiko.Bot.Games;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services;
using Yumiko.Bot.Services.State;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces;

namespace Yumiko.Bot.Commands.Slash;

[TestCommand]
[RequireGuild]
public sealed class Games(
    ILocalizer localizer,
    DiscordBotService discordBotService,
    TriviaState triviaState,
    GamePool pool,
    GenreSelector genreSelector,
    TriviaGameRunner triviaRunner,
    HangmanGameRunner hangmanRunner,
    HigherOrLowerGameRunner higherOrLowerRunner,
    TicTacToeGameRunner ticTacToeRunner,
    InteractivityExtension interactivity,
    IAnilistClient anilist,
    GamesSettings gamesSettings)
{
    [Command("trivia")]
    [Description("Plays an anime trivia game")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task TriviaAsync(
        SlashCommandContext ctx,
        [Parameter("gamemode")] [Description("The type of game you want to play")] GamemodeChoice gamemodeChoice,
        [Parameter("difficulty")] [Description("Choose the difficulty of the trivia")] DifficultyChoice difficultyChoice,
        [Parameter("rounds")] [Description("Rounds to play (minimum is 1 and maximum is 30)")] [MinMaxValue(1, 30)] long rounds)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await ctx.EnsureBotReadyAsync(discordBotService, loc))
        {
            return;
        }

        if (triviaState.Get(ctx.Guild!.Id, ctx.Channel.Id) is not null)
        {
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
                .AsEphemeral()
                .AddEmbed(ErrorEmbed.Create(loc[Keys.another_trivia_playing], loc[Keys.another_trivia_playing_desc])));
            return;
        }

        Gamemode gamemode = gamemodeChoice.ToModel();
        Difficulty difficulty = difficultyChoice.ToModel();
        (int pageFrom, int pageTo) = MediaPoolBuilder.DifficultyRange(difficulty);

        GameSettings settings = new()
        {
            Ok = true,
            Rounds = (int)rounds,
            Difficulty = difficulty,
            Gamemode = gamemode,
            PageFrom = pageFrom,
            PageTo = pageTo,
        };

        await ctx.DeferResponseAsync();

        if (gamemode == Gamemode.Genres)
        {
            if (await genreSelector.ChooseAsync(ctx, loc) is not { } genre)
            {
                await ctx.EditResponseAsync(ErrorEmbed.Create(loc[Keys.error], loc[Keys.timed_out_choosing_genre]));
                return;
            }

            settings.Genre = genre;
        }

        string gameLabel = GameName(settings, loc);

        await ctx.EditResponseAsync(new DiscordEmbedBuilder
        {
            Title = GameTitle(gamemode, loc),
            Color = YumikoColors.Primary,
        }
        .AddField(loc[Keys.rounds], $"{settings.Rounds}")
        .AddField(
            gamemode == Gamemode.Genres ? loc[Keys.genre] : loc[Keys.difficulty],
            gamemode == Gamemode.Genres ? settings.Genre! : GameStatsEmbeds.DifficultyLabel(difficulty, loc)));

        List<TriviaItem> items = await BuildPoolAsync(ctx, settings, loc);

        await triviaRunner.PlayAsync(ctx, items, settings, gameLabel, loc);
    }

    [Command("hangman")]
    [Description("Plays the hangman game")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task HangmanAsync(
        SlashCommandContext ctx,
        [Parameter("game")] [Description("If the game is about anime characters or anime titles")] HangmanGamemodeChoice gamemodeChoice)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await ctx.EnsureBotReadyAsync(discordBotService, loc))
        {
            return;
        }

        await ctx.DeferResponseAsync();

        HangmanGamemode gamemode = gamemodeChoice.ToModel();
        int page = RandomHelper.GetRandomNumber(1, gamesSettings.RandomPageMax);
        HangmanTarget? target = gamemode == HangmanGamemode.Characters
            ? FromCharacter(await anilist.GetRandomCharacterSimpleAsync(page), loc)
            : FromAnime(await anilist.GetRandomMediaAsync(page, MediaType.ANIME), loc);

        if (target is null)
        {
            await ctx.EditResponseAsync(ErrorEmbed.Unknown(loc));
            return;
        }

        await hangmanRunner.PlayAsync(ctx, target, loc);
    }

    [Command("higherorlower")]
    [Description("Plays a Higher or Lower game")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task HigherOrLowerAsync(
        SlashCommandContext ctx,
        [Parameter("gamemode")] [Description("Higher or Lower gamemode")] GamemodeHoLChoice gamemodeChoice)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await ctx.EnsureBotReadyAsync(discordBotService, loc))
        {
            return;
        }

        GamemodeHoL gamemode = gamemodeChoice.ToModel();

        await ctx.RespondAsync(new DiscordEmbedBuilder
        {
            Title = "Higher or Lower",
            Description = loc[gamemode == GamemodeHoL.Score ? Keys.higher_or_lower_desc : Keys.higher_or_lower_desc_popularity],
            Color = YumikoColors.Primary,
        });

        await higherOrLowerRunner.PlayAsync(ctx, gamemode, interactivity, loc);
    }

    [Command("tictactoe")]
    [Description("Starts a Tic-Tac-Toe game")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task TicTacToeAsync(
        SlashCommandContext ctx,
        [Parameter("player2")] [Description("The second player of the game")] DiscordUser player2)
    {
        Loc loc = ctx.Loc(localizer);

        if (player2.IsBot)
        {
            await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
                .AsEphemeral()
                .AddEmbed(ErrorEmbed.Create(loc[Keys.error], loc[Keys.cant_play_vs_bot])));
            return;
        }

        await ctx.DeferResponseAsync();
        await ticTacToeRunner.PlayAsync(ctx, player2, loc);
    }

    private async Task<List<TriviaItem>> BuildPoolAsync(SlashCommandContext ctx, GameSettings settings, Loc loc)
    {
        if (settings.Gamemode == Gamemode.Characters)
        {
            return TriviaItems.FromCharacters(await pool.GetCharactersAsync(settings.PageFrom, settings.PageTo), loc);
        }

        GameMediaQuery query = new()
        {
            Type = settings.Gamemode == Gamemode.Mangas ? MediaType.MANGA : MediaType.ANIME,
            Genre = settings.Genre,
            IncludeAdult = ctx.Channel.IsNSFW,
            IncludeCharacters = settings.Gamemode == Gamemode.Protagonists,
            IncludeStudios = settings.Gamemode == Gamemode.Studios,
            IncludeRelatedMedia = settings.Gamemode is Gamemode.Animes or Gamemode.Genres,
        };

        (int pageFrom, int pageTo) = settings.Gamemode == Gamemode.Genres
            ? await pool.GetGenreRangeAsync(query)
            : (settings.PageFrom, settings.PageTo);

        return TriviaItems.FromMedia(await pool.GetMediaAsync(query, pageFrom, pageTo), settings.Gamemode, loc);
    }

    private static string GameTitle(Gamemode gamemode, Loc loc) => gamemode switch
    {
        Gamemode.Characters => loc[Keys.guess_the_character],
        Gamemode.Animes => loc[Keys.guess_the_anime],
        Gamemode.Mangas => loc[Keys.guess_the_manga],
        Gamemode.Studios => loc[Keys.guess_the_studio],
        Gamemode.Protagonists => loc[Keys.guess_the_protagonist],
        _ => loc[Keys.guess_the_genre],
    };

    /// <summary>
    /// What is read after "guess the". In Spanish the enums have their own translation; in
    /// English the plural "s" of the enum is dropped, except for the two cases that cannot be derived.
    /// </summary>
    private static string GameName(GameSettings settings, Loc loc)
    {
        if (loc.IsSpanish)
        {
            return settings.Gamemode.ToSpanish();
        }

        return settings.Gamemode switch
        {
            Gamemode.Genres => settings.Genre ?? $"{settings.Gamemode}",
            Gamemode.Studios => $"{loc[Keys.studio]} {loc[Keys.from_the_anime]}",
            _ => $"{settings.Gamemode}"[..^1],
        };
    }

    private static HangmanTarget? FromCharacter(CharacterOld? character, Loc loc) =>
        character?.NameFull is null
            ? null
            : new HangmanTarget(
                character.NameFull,
                character.Image,
                loc.Format(
                    Keys.the_character_is,
                    DSharpPlus.Formatter.Bold($"[{character.NameFull}]({character.SiteUrl})"),
                    $"[{character.MainAnime?.TitleRomaji}]({character.MainAnime?.SiteUrl})"),
                $"{HangmanGamemode.Characters}");

    private static HangmanTarget? FromAnime(Anime? anime, Loc loc) =>
        anime is null
            ? null
            : new HangmanTarget(
                anime.TitleRomaji,
                anime.Image,
                loc.Format(Keys.the_anime_is, DSharpPlus.Formatter.Bold($"[{anime.TitleRomaji}]({anime.SiteUrl})")),
                $"{HangmanGamemode.Animes}");
}
