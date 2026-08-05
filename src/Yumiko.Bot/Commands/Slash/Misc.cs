using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Entities;
using Yumiko.Application.Helpers;
using Yumiko.Bot.Commands.Framework;
using Yumiko.Bot.Commands.Framework.Attributes;
using Yumiko.Bot.Commands.Framework.AutoComplete;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Extensions;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces;

namespace Yumiko.Bot.Commands.Slash;

[TestCommand]
public sealed class Misc(
    ILocalizer localizer,
    DiscordBotService discordBotService,
    IWeatherClient weatherClient,
    IAnimalImageClient animalImageClient,
    BotConfiguration config,
    TopggService topgg)
{
    private static readonly DiscordPermission[] InvitationPermissions =
    [
        DiscordPermission.ViewChannel,
        DiscordPermission.SendMessages,
        DiscordPermission.SendThreadMessages,
        DiscordPermission.UseExternalEmojis,
    ];

    [Command("ping")]
    [Description("Shows Yumiko's ping")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task PingAsync(SlashCommandContext ctx)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await ctx.EnsureBotReadyAsync(discordBotService, loc))
        {
            return;
        }

        await ctx.RespondAsync(new DiscordEmbedBuilder
        {
            Title = "Ping",
            Description = $"🏓 Pong! `{ctx.Client.GetConnectionLatency(ctx.Guild?.Id ?? 0).TotalMilliseconds:0} ms`",
            Color = YumikoColors.Primary,
        });
    }

    [Command("weather")]
    [Description("Shows the weather in a location")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task WeatherAsync(
        SlashCommandContext ctx,
        [Parameter("location")] [Description("City where you want to search the weather")] string city,
        [Parameter("country")] [Description("Country where you want to search the weather")] [SlashAutoCompleteProvider<CountriesAutoCompleteProvider>] string country)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await ctx.EnsureBotReadyAsync(discordBotService, loc))
        {
            return;
        }

        await ctx.DeferResponseAsync();

        Weather? weather = await weatherClient.GetWeatherAsync(city, country, loc.IsSpanish ? "es" : "en");

        if (weather is null)
        {
            await ctx.EditResponseAsync(ErrorEmbed.Create(loc[Keys.error], loc.Format(Keys.location_not_found, city, country)));
            return;
        }

        // The weather values go unlocalized: they are units, not text.
        CultureInfo ic = CultureInfo.InvariantCulture;

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
        {
            Title = $"{loc[Keys.weather_in]} {weather.CityName}",
            Url = $"https://openweathermap.org/city/{weather.CityId}",
            Color = YumikoColors.Primary,
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"{loc[Keys.retrieved_from]} openweathermap.org",
                IconUrl = "https://openweathermap.org/img/w/03d.png",
            },
        }
        .AddField($":cloud: {loc[Keys.weather]}", weather.Description.UppercaseFirst(), true)
        .AddField($":sweat: {loc[Keys.humidity]}", $"{weather.Humidity.ToString(ic)}%", true)
        .AddField($":ocean: {loc[Keys.pressure]}", $"{weather.Pressure.ToString(ic)} hPa", true)
        .AddField($":dash: {loc[Keys.wind_speed]}", $"{weather.WindSpeed.ToString(ic)} m/s", true)
        .AddField($":thermometer: {loc[Keys.temperature]}", $"{weather.Temperature.ToString(ic)} °C", true)
        .AddField($":thermometer_face: {loc[Keys.feels_like]}", $"{weather.FeelsLike.ToString(ic)} °C", true)
        .AddField($":high_brightness: {loc[Keys.min_max_temperature]}", $"{weather.TemperatureMin.ToString(ic)} °C - {weather.TemperatureMax.ToString(ic)} °C", true)
        .AddField($":sunrise_over_mountains: {loc[Keys.sunrise]}", $"<t:{weather.Sunrise}:t>", true)
        .AddField($":city_sunset: {loc[Keys.sunset]}", $"<t:{weather.Sunset}:t>", true);

        await ctx.EditResponseAsync(embed);
    }

    [Command("cat")]
    [Description("Random kitten")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public Task CatAsync(SlashCommandContext ctx) =>
        RespondWithAnimalAsync(ctx, AnimalKind.Cat, Keys.random_cat, Keys.random_cat_error, "(๑✪ᆺ✪๑)");

    [Command("dog")]
    [Description("Random dog")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public Task DogAsync(SlashCommandContext ctx) =>
        RespondWithAnimalAsync(ctx, AnimalKind.Dog, Keys.random_dog, Keys.random_dog_error, "(❍ᴥ❍ʋ)");

    private async Task RespondWithAnimalAsync(SlashCommandContext ctx, AnimalKind kind, string titleKey, string errorKey, string kaomoji)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await ctx.EnsureBotReadyAsync(discordBotService, loc))
        {
            return;
        }

        await ctx.DeferResponseAsync();

        byte[]? image = await animalImageClient.GetRandomImageAsync(kind);

        if (image is null)
        {
            await ctx.EditResponseAsync(ErrorEmbed.Create(loc[Keys.unknown_error], $"{loc[errorKey]} :c"));
            return;
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(new DiscordEmbedBuilder
            {
                Title = $"{loc[titleKey]} {kaomoji}",
                ImageUrl = "attachment://image.png",
                Color = YumikoColors.Primary,
            })
            .AddFile("image.png", image.ToMemoryStream()));
    }

    [Command("help")]
    [Description("Help and information about Yumiko")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task HelpAsync(SlashCommandContext ctx)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await ctx.EnsureBotReadyAsync(discordBotService, loc))
        {
            return;
        }

        await ctx.DeferResponseAsync();

        string description = Formatter.BlockCode(loc.Format(Keys.bot_about, ctx.Client.CurrentUser.Username)) + "\n";

        IEnumerable<IGrouping<string, Command>> categories = ctx.Extension.Commands.Values
            .Where(IsPublicSlashCommand)
            .GroupBy(Category)
            .OrderBy(category => category.Key, StringComparer.Ordinal);

        foreach (IGrouping<string, Command> category in categories)
        {
            description += $"\n{Formatter.Bold(category.Key)}\n";
            description += string.Concat(category
                .OrderBy(c => c.Name, StringComparer.Ordinal)
                .Select(FormatCommand));
        }

        List<DiscordLinkButtonComponent> buttons =
        [
            new(InvitationUri(ctx), loc[Keys.invite]),
            new(config.Website, loc[Keys.website]),
        ];

        if (topgg.Enabled)
        {
            buttons.Add(new DiscordLinkButtonComponent($"https://top.gg/bot/{ctx.Client.CurrentApplication.Id}/vote", loc[Keys.vote]));
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(new DiscordEmbedBuilder
            {
                Title = $"{loc[Keys.about]} {ctx.Client.CurrentUser.Username}",
                Description = description.NormalizeDescription(),
                Color = YumikoColors.Primary,
            })
            .AddActionRowComponent(buttons));
    }

    /// <summary>
    /// Only what anybody can type: the context menus are left out because they are not typed, and
    /// so are the commands restricted to the logs guild, which nobody else can run.
    /// </summary>
    private static bool IsPublicSlashCommand(Command command) =>
        DeclaringType(command)?.GetCustomAttribute<LogGuildOnlyAttribute>() is null
        && command.Attributes.OfType<SlashCommandTypesAttribute>().All(a =>
            a.ApplicationCommandTypes.Contains(DiscordApplicationCommandType.SlashCommand));

    /// <summary>
    /// The category is the class the command is declared in, which is what groups <c>/trivia</c> and
    /// <c>/hangman</c> under Games without them being a command group.
    /// </summary>
    private static string Category(Command command) =>
        DeclaringType(command)?.Name ?? command.Name.UppercaseFirst();

    private static Type? DeclaringType(Command command) =>
        command.Method?.DeclaringType ?? command.Subcommands.FirstOrDefault()?.Method?.DeclaringType;

    /// <summary>
    /// A group lists one line per subcommand; a standalone command, a single line.
    /// The context menus are left out: they are not typed, they are used from the context menu.
    /// </summary>
    private static string FormatCommand(Command command)
    {
        if (command.Subcommands.Count == 0)
        {
            return $"{Formatter.InlineCode($"/{command.Name}")} {command.Description}\n";
        }

        return string.Concat(command.Subcommands
            .OrderBy(sub => sub.Name, StringComparer.Ordinal)
            .Select(sub => $"{Formatter.InlineCode($"/{command.Name} {sub.Name}")} {sub.Description}\n"));
    }

    private static string InvitationUri(SlashCommandContext ctx) =>
        ctx.Client.CurrentApplication
            .GenerateOAuthUri(null, new DiscordPermissions(InvitationPermissions), DiscordOAuthScope.Bot, DiscordOAuthScope.ApplicationsCommands)
            .Replace(" ", "%20");
}
