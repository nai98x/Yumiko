# CLAUDE.md

Guía para trabajar en este repo. El README tiene el detalle de arquitectura y el mapa del código; acá
van las **convenciones** a respetar al editar.

## Regla absoluta: nunca commitear ni pushear

**NUNCA** ejecutes `git commit`, `git push` ni ninguna operación que escriba en el historial o el
remoto. Los commits y pushes los hace **exclusivamente el dueño del repo, a mano**. Podés editar
archivos, correr builds/tests y preparar cambios, pero el versionado lo controla siempre la persona.

## Qué es

Bot de Discord **público y multi-guild** (DSharpPlus, .NET 10), centrado en consultas a AniList, más
juegos, comandos de interacción y utilidades. Está listado en top.gg y corre con sharding.
Persistencia en **PostgreSQL** (Dapper + stored procedures).

Es **bilingüe EN/ES**: cada texto de usuario existe en los dos idiomas y se resuelve por el locale de
la interacción.

**Idioma del código**: **todo el código va en inglés**, sin excepción: identificadores, comentarios,
XML summaries, mensajes de log, mensajes de excepción y nombres de tests. No debe quedar nada en
español dentro de `src/` ni `tests/`. La única excepción son los textos de usuario de
`Translations.es.resx` (que por definición son la traducción al español), incluidas las etiquetas en
español de `GameNaming` (`personaje`, `Fácil`, `Dificil`).

Los commits y la comunicación con el dueño del repo siguen siendo en **español**.

## Arquitectura (Clean Architecture, 4 proyectos en `src/`)

Dirección de dependencias: **Model ← Application ← Infrastructure ← Bot**. Nunca agregar referencias
inversas.

- **Yumiko.Model** — entidades, enums, excepciones e interfaces (`IAnilistClient`, `IWeatherClient`,
  `Interfaces/Repositories/*`). POCOs puros, **cero paquetes NuGet**.
- **Yumiko.Application** — lógica de negocio y cálculo puro. Sin dependencias de Discord ni de
  infraestructura.
- **Yumiko.Infrastructure** — repositorios PostgreSQL (Dapper), `DbConnectionFactory`, cliente de AniList y los
  clientes HTTP tipados (weather, cat/dog, trace.moe, AnimeThemes, top.gg).
- **Yumiko.Bot** — entry point, comandos, handlers, scheduling, estado en memoria, configuración, DI.

## Dónde va el código nuevo

- **Regla/cálculo puro** (sin estado ni dependencias) → clase **estática** en Application
  (`Games/`, `Anilist/`, `Fun/`, `Helpers/`). Recibe sus datos **por parámetro**; no toca tipos de
  Discord ni de configuración.
- **Servicio con dependencias** → clase instancia + DI (ej. `RecommendationService` en Application;
  `GamePool`, `AnilistResponses`, los `*GameRunner` en Bot).
- **El "seam" con Discord queda en Bot**: embeds, botones, interactividad y el ruteo de componentes
  son responsabilidad del Bot; las reglas (tablero de ta-te-ti, estado del ahorcado, ranking de
  trivia, scoring de recomendaciones) van en Application.
- Preferí mover lógica fuera de los comandos hacia Application antes que engordarlos. No
  sobre-ingenierizar: un predicado de una línea de orquestación puede quedar en Bot.

## Base de datos (PostgreSQL)

La base es **database-first**: el esquema vive en `db/` (ver `db/README.md`) y es la fuente de
verdad; el C# se adapta. Tablas, columnas y funciones **en inglés**, como el resto del código.

- **Nunca SQL embebido en el código.** El acceso es con **Dapper invocando stored procedures**
  (`CommandType.StoredProcedure`); un `.sql` por SP en `db/procedures/`, con el nombre del SP como
  nombre de archivo. Cambio de esquema y código que lo usa van en el mismo commit.
- **Repositorios**: inyectar SIEMPRE por su interfaz de Model (`IXxxRepository`), nunca la clase
  concreta de Infrastructure. Solo las interfaces están registradas en el contenedor.
- Los POCOs de Model **no** conocen la base. El mapeo vive en `Infrastructure/Database/Rows/*`
  (DTOs de fila locales a esa capa) + mappers en el repositorio.
- Un **único `NpgsqlDataSource` de larga vida** vía `DbConnectionFactory` (singleton). Nunca
  construir uno por llamada; sí abrir y cerrar una conexión por operación.
- Los ids de Discord son `ulong` en C# y `bigint` en la base: castear a `long` al pasar el parámetro
  y volver a `ulong` al leer.
- `quiz_stats.accuracy_percentage` se guarda con **división entera**. Pasarlo a decimal re-rankea
  todos los leaderboards que ya existen. No cambiar.
- En modo `Genres`, la columna `difficulty` guarda el **nombre del género**, no una dificultad.
- `gamemode` y `difficulty` guardan los **nombres de los enums** (`Characters`, `Easy`), nunca las
  etiquetas en español de `GameNaming`: esas son solo para mostrar.

## Bilingüe

- Los strings de usuario viven **solo** en `Bot/Resources/Translations.resx` (+ `.es.resx`), con las
  claves en `Localization/Keys.cs`. Model y Application **nunca** devuelven texto de usuario.
- Siempre resolver por un `Loc` explícito, obtenido con `ctx.Loc(localizer)` desde el locale de la
  interacción. **Nunca** `Thread.CurrentUICulture`, y menos dentro de un `Task.Run`: `Loc` es un
  struct justamente para capturarse por valor.
- Todo comando nuevo lleva `[InteractionLocalizer<ResxInteractionLocalizer>]` (sin argumento: la
  clave es `Command.FullName + ".name"` / `".description"`).
- Agregar una clave implica agregarla a **los dos** `.resx`. Las claves con punto no entran en
  `Keys.cs` porque no son identificadores C# válidos: son convención de DSharpPlus y se referencian
  solas desde el atributo.

## Convenciones clave

- **DI**: por **constructor** (primary constructors). No usar service locator
  (`IServiceProvider.GetService`) salvo los dos casos ya establecidos: `EventHandlerRegistrar`, donde
  la resolución se difiere dentro de un lambda para romper el ciclo
  `DiscordClient → Handler → DiscordBotService → DiscordClient`, y los providers de autocomplete, que
  reciben el provider en su contexto.
- **Configuración** (ver `Bot/Configuration/`):
  - IDs de Discord → `BotConfiguration` + sección `Ids` de `appsettings.json`. `RequireUlong` falla
    al arrancar si falta una clave.
  - Reglas tuneables → **una sección por dominio** en `appsettings.json` (`Timeouts`, `Logs`,
    `Topgg`, `Games`), bindeadas en `BehaviorSettings.cs` con defaults. Application recibe
    esos valores **por parámetro**, nunca como `const`.
  - Las diferencias Debug/Release van en `appsettings.Development.json`, **no** en claves prod/test
    duplicadas.
- **Secrets**: `discordToken`, `ConnectionStrings:Database`, `openWeatherMapToken`, `theCatApiToken`,
  `theDogApiToken`, `AnilistApiClientId`, `topggToken`. User Secrets en local, variables de entorno
  en el server. Nada de esto se versiona. Detalle en `deploy-setup/README.md`.
- **Estado en memoria**: `Bot/Services/State/*`, siempre thread-safe (`ConcurrentDictionary` o swap
  de un campo `volatile`). Se muta desde hilos del gateway.
- **Aleatoriedad**: `RandomHelper` / `Random.Shared`, salvo donde se necesita determinismo por
  semilla (`/waifu real`, `/love real`), que se deja intacto.
- **Catch amplios**: la resiliencia de los loops de juego es intencional. Loguear con
  `DiscordLogService` sin cambiar el flujo.
- **Estilo**: **no** agregar comentarios ni XML summaries explicando *cómo se resolvió* algo o
  justificando un cambio. Comentar solo lo no obvio del dominio; las rarezas de la base de arriba
  califican. Los comentarios que se escriban van **en inglés** (ver "Idioma del código").

## Tests

`tests/Yumiko.Application.Tests` referencia **solo** `Yumiko.Application`. La estructura de carpetas
espeja la del fuente. Determinismo por semilla; los dobles de las interfaces de Model se escriben a
mano (no hay librería de mocking) y tiran en los métodos que el test no debería llamar.

La cobertura se mide acotada a Application con `tests/Yumiko.Application.Tests/coverlet.runsettings`:

```bash
dotnet test --settings tests/Yumiko.Application.Tests/coverlet.runsettings --collect:"XPlat Code Coverage"
```

## Dependencias / entorno

- DSharpPlus usa paquetes **nightly** (`5.0.0-nightly-02593`) — habilitar versiones preliminares al
  restaurar. Las versiones compartidas deben coincidir exactamente con las de AnilistConEnie.
- `DiscordShardedClient` no existe: el sharding se arma con `AddShardedDiscordClient` y queda detrás
  de un único `DiscordClient`. **No fijar `ShardingOptions.ShardCount`**: con el default (null) el
  orquestador usa el que recomienda Discord, que crece con la cantidad de guilds.
- `GetInteractivity()` no existe: resolver `InteractivityExtension` por DI.
- `DiscordLocale.es_419` no existe en este nightly; solo `es_ES`.
- Está permitido leer internals/decompilados de DSharpPlus para entender comportamiento.

## Comandos

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Yumiko.Bot
```
