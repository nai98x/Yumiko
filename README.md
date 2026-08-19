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

Bot de Discord público y multi-guild centrado en **AniList**: buscar animes, mangas, personajes y
staff, vincular tu perfil, sacar recomendaciones automáticas de tu lista. Además tiene juegos
(trivia, ahorcado, higher-or-lower, ta-te-ti), comandos de interacción y utilidades.

Es **bilingüe**: responde en inglés o español según el idioma que cada usuario tenga configurado en
Discord, incluidos los nombres y descripciones de los comandos.

## Stack

.NET 10 · DSharpPlus 5 (nightly) · PostgreSQL (Dapper) · SkiaSharp · Serilog · xUnit

## Arquitectura

Clean Architecture, cuatro proyectos en `src/`. La dirección de dependencias es
**Model ← Application ← Infrastructure ← Bot** y nunca al revés.

| Proyecto | Qué contiene | Depende de |
|---|---|---|
| `Yumiko.Model` | Entidades, enums, excepciones e interfaces. POCOs puros, **cero paquetes NuGet**. | — |
| `Yumiko.Application` | Reglas de negocio y cálculo puro: juegos, scoring de recomendaciones, formateo de puntajes, imágenes. Sin Discord ni I/O. | Model |
| `Yumiko.Infrastructure` | Repositorios PostgreSQL (Dapper + stored procedures), cliente de AniList (GraphQL + Polly) y clientes HTTP tipados. | Model, Application |
| `Yumiko.Bot` | Entry point, comandos, handlers, scheduling, estado en memoria, localización, DI. | las tres |

`tests/Yumiko.Application.Tests` referencia **solo** `Yumiko.Application`.

## Mapa del código

```
src/Yumiko.Model/
  Entities/            Anilist/, Games/, AnimeThemes/, Weather, Poll, Country...
  Enum/                Difficulty, Gamemode, GamemodeHoL, MediaType...
  Exceptions/          AnilistApiException y derivadas, TraceMoeQuotaException
  Interfaces/          IAnilistClient, IWeatherClient, ITopggClient... + Repositories/

src/Yumiko.Application/
  Anilist/             RecommendationScoring, RecommendationService, ScoreFormatter
  Games/               TicTacToe, HangmanState, HigherOrLower, TriviaScoring,
                       TriviaRound, LeaderboardRanking, MediaPoolBuilder, GameNaming
  Fun/                 LoveMeter
  Helpers/             TextHelper, ImageHelper (SkiaSharp), EmojiHelper, RandomHelper

src/Yumiko.Infrastructure/
  Anilist/             AnilistClient, AnilistGraphQLExecutor (Polly), AnilistQueries, Responses/
  Database/            DbConnectionFactory (Npgsql + Dapper), Rows/ (DTOs de fila)
  Repositories/        QuizLeaderboard, HigherOrLowerLeaderboard, AnilistUsers
  OpenWeather/ Animals/ TraceMoe/ AnimeThemes/ Topgg/

src/Yumiko.Bot/
  Commands/Slash/      Anilist, Games, Interact, Misc, Owner, Stats
  Commands/ContextMenu/ AnilistProfile, AnimeRecommendations, MangaRecommendations
  Commands/Framework/  Choices/, AutoComplete/, CommandErrorHandler, ResxInteractionLocalizer
  Games/               Runners de los 4 juegos, Poll, Trivia, GamePool, TriviaItems
  Helpers/             Embeds, DiscordInteractivity, DiscordLogService, TopggService...
  Events/              EventHandlerRegistrar + Handlers/
  Services/            DiscordBotService, MediaCacheRefresher, Scheduling/, State/
  Localization/        ILocalizer, ResxLocalizer, Loc, Keys
  Resources/           Translations.resx (+ .es), countries.json
  Configuration/       BotConfiguration, BehaviorSettings, BotEnvironment

db/
  schema/              anilist_users, higher_or_lower_scores, quiz_stats
  procedures/          un .sql por stored procedure
```

El esquema de la base es la fuente de verdad y se versiona en [`db/`](db/README.md): el acceso desde
el bot es siempre por stored procedures invocados con Dapper, nunca con SQL embebido en el código.

## Comandos

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Yumiko.Bot
```

Antes de correrlo hace falta configurar los secrets (User Secrets en local) y tener la base creada
con los scripts de `db/` aplicados: ver [`deploy-setup/README.md`](deploy-setup/README.md).

En build **Debug** los comandos se registran solo en el guild de `Ids:LogGuildId`, así que se puede
probar sin tocar la instancia pública.

## Deploy

Automático por GitHub Actions al pushear a `master`: build → tests → escaneo de paquetes vulnerables
→ `publish` para `linux-arm64` → SCP al server → reinicio con `systemctl --user`. El detalle y el
setup inicial están en [`deploy-setup/README.md`](deploy-setup/README.md).
