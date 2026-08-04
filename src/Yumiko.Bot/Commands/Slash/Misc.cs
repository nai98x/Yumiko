using System.ComponentModel;
using System.Globalization;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Entities;
using Yumiko.Application.Helpers;
using Yumiko.Bot.Commands.Framework;
using Yumiko.Bot.Commands.Framework.Attributes;
using Yumiko.Bot.Commands.Framework.AutoComplete;
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
    IAnimalImageClient animalImageClient)
{
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

        // Los valores meteorológicos van sin localizar: son unidades, no texto.
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
}
