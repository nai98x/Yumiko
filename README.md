<p align="center"><img src="https://i.imgur.com/sNUQoqf.png" width="200px" height="200px"></p>
<h1 align="center">Yumiko</h1>
<p align="center">
  Discord Bot written on top of DSharpPlus
  <br><br>
  <a href="https://www.codefactor.io/repository/github/nai98x/yumiko" target="_blank"><img src="https://www.codefactor.io/repository/github/nai98x/yumiko/badge?s=92181f030fc6101fb54afa74167809713aa4d060" alt="Codefactor"></a>
  <a href="https://github.com/nai98x/yumiko/actions/workflows/deploy.yml" target="_blank"><img src="https://github.com/nai98x/yumiko/actions/workflows/deploy.yml/badge.svg?branch=master" alt="CI/CD"></a>
  <a><img src="https://img.shields.io/github/languages/code-size/nai98x/Yumiko?style=?style=plastic&color=blueviolet" alt="Code size"></a>
  <br>
  <img alt="Bot status" src="https://img.shields.io/website?down_color=red&down_message=offline&label=Bot%20Status&up_color=green&up_message=Online&url=https%3A%2F%2Fyumiko.uwu.ai%2F">
  <a href="https://top.gg/bot/295182825521545218" target="_blank"><img src="https://top.gg/api/widget/servers/295182825521545218.svg?noavatar=true" alt="Top.gg"></a>
  <a href="https://top.gg/bot/295182825521545218/vote" target="_blank"><img src="https://top.gg/api/widget/upvotes/295182825521545218.svg?noavatar=true" alt="Top.gg"></a>
  <br><br>
  <a href="https://discord.gg/nhabKQ5FS8" target="_blank"><img src="https://discord.com/api/guilds/713809173573271613/embed.png?style=banner2" alt="Yumiko support server"></a>
</p>

---

Public, multi-guild Discord bot centred on **AniList**: search animes, mangas, characters and staff,
link your profile, get automatic recommendations out of your list. It also has games (trivia,
hangman, higher-or-lower, tic-tac-toe), interaction commands and utilities.

It is **bilingual**: it answers in English or Spanish depending on the language each user has set in
Discord, command names and descriptions included.

## Stack

.NET 10 · DSharpPlus 5 (nightly) · PostgreSQL (Dapper) · SkiaSharp · Serilog · xUnit

## Architecture

Clean Architecture, four projects under `src/`. Dependencies flow
**Model ← Application ← Infrastructure ← Bot** and never the other way around.

| Project | What it holds | Depends on |
|---|---|---|
| `Yumiko.Model` | Entities, enums, exceptions and interfaces. Plain POCOs, **zero NuGet packages**. | — |
| `Yumiko.Application` | Business rules and pure computation: games, recommendation scoring, score formatting, images. No Discord, no I/O. | Model |
| `Yumiko.Infrastructure` | PostgreSQL repositories (Dapper + stored procedures), the AniList client (GraphQL + Polly) and the typed HTTP clients. | Model, Application |
| `Yumiko.Bot` | Entry point, commands, handlers, scheduling, in-memory state, localization, DI. | all three |

`tests/Yumiko.Application.Tests` references **only** `Yumiko.Application`.

## Code map

```
src/Yumiko.Model/
  Entities/            Anilist/, Games/, AnimeThemes/, Weather, Poll, Country...
  Enum/                Difficulty, Gamemode, GamemodeHoL, MediaType...
  Exceptions/          AnilistApiException and derived, TraceMoeQuotaException
  Interfaces/          IAnilistClient, IWeatherClient, ITopggClient... + Repositories/

src/Yumiko.Application/
  Anilist/             RecommendationScoring, RecommendationService, ScoreFormatter
  Games/               TicTacToe, HangmanState, HigherOrLower, TriviaScoring,
                       TriviaRound, LeaderboardRanking, MediaPoolBuilder, GameNaming
  Fun/                 LoveMeter
  Helpers/             TextHelper, ImageHelper (SkiaSharp), EmojiHelper, RandomHelper
  Backups/             BackupState

src/Yumiko.Infrastructure/
  Anilist/             AnilistClient, AnilistGraphQLExecutor (Polly), AnilistQueries, Responses/
  Database/            DbConnectionFactory (Npgsql + Dapper), Rows/ (row DTOs)
  Repositories/        QuizLeaderboard, HigherOrLowerLeaderboard, AnilistUsers
  OpenWeather/ Animals/ TraceMoe/ AnimeThemes/ Topgg/

src/Yumiko.Bot/
  Commands/Slash/      Anilist, Games, Interact, Misc, Owner, Stats
  Commands/ContextMenu/ AnilistProfile, AnimeRecommendations, MangaRecommendations
  Commands/Framework/  Choices/, AutoComplete/, CommandErrorHandler, ResxInteractionLocalizer
  Games/               Runners of the 4 games, Poll, Trivia, GamePool, TriviaItems
  Helpers/             Embeds, DiscordInteractivity, DiscordLogService, TopggService...
  Events/              EventHandlerRegistrar + Handlers/
  Services/            DiscordBotService, MediaCacheRefresher, Scheduling/, State/
  Localization/        ILocalizer, ResxLocalizer, Loc, Keys
  Resources/           Translations.resx (+ .es), countries.json
  Configuration/       BotConfiguration, BehaviorSettings, BotEnvironment

db/
  schema/              anilist_users, higher_or_lower_scores, quiz_stats
  procedures/          one .sql per stored procedure
```

The database schema is the source of truth and lives in [`db/`](db/README.md): the bot always goes
through stored procedures invoked with Dapper, never through SQL embedded in the code.

## Commands

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Yumiko.Bot
```

Before running it you need the secrets configured (User Secrets locally) and the database created
with the scripts of `db/` applied: see [`deploy-setup/README.md`](deploy-setup/README.md).

On a **Debug** build the commands are registered only on the `Ids:LogGuildId` guild, so it can be
tested without touching the public instance.

## Deploy

Automatic through GitHub Actions on every push to `master`: build → tests → vulnerable package scan
→ `publish` for `linux-arm64` → SCP to the server → restart with `systemctl --user`. The details and
the initial setup are in [`deploy-setup/README.md`](deploy-setup/README.md).
