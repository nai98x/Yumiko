namespace Yumiko.Commands
{
    using Newtonsoft.Json;
    using RestSharp;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading.Tasks;
    using System.Web;

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Not with D#+ Command classes")]
    public class Misc : ApplicationCommandModule
    {
        public IConfigurationRoot Configuration { private get; set; } = null!;

        public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(ctx.Interaction.Locale!);
            return Task.FromResult(true);
        }

        [SlashCommand("ping", "Shows Yumiko´s ping")]
        public async Task Ping(InteractionContext ctx)
        {
            Common.GetFirestoreClient();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = "Ping",
                Description = "🏓 Pong! `" + ctx.Client.Ping.ToString() + " ms" + "`",
                Color = Constants.YumikoColor,
            }));
        }

        [SlashCommand("weather", "Shows the weather in your location")]
        public async Task Weather(
            InteractionContext ctx,
            [Option("Location", "City where you want to search the weather")] string localidad,
            [Option("Country", "Country where you want to search the weather", true)][Autocomplete(typeof(CountriesAutocompleteProvider))] string pais)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            string baseUrl = "https://api.openweathermap.org/data/2.5/weather";
            string language = ctx.Interaction.Locale! switch
            {
                "es-ES" => "es",
                _ => "en"
            };
            var client = new RestClient(baseUrl + $"?q={HttpUtility.UrlEncode(localidad)},{pais}&appid={ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.TokenOpenWeatherMap)}&lang={language}&units=metric");
            var request = new RestRequest();

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful && response.Content != null)
            {
                var data = JsonConvert.DeserializeObject<dynamic>(response.Content);

                if (data != null)
                {
                    string localidadNombre = data.name;
                    string localidadId = data.id;
                    string localidadUrl = "https://openweathermap.org/city/" + localidadId;
                    string clima = data.weather[0].description;
                    string humedad = data.main.humidity;
                    string presion = data.main.pressure;
                    string viento = data.wind.speed;
                    string temperatura = data.main.temp;
                    string min = data.main.temp_min;
                    string max = data.main.temp_max;
                    string sunrise = data.sys.sunrise;
                    string sunset = data.sys.sunset;
                    string sensasionTermica = data.main.feels_like;

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"{strings.weather_in} {localidadNombre}",
                        Url = localidadUrl,
                        Color = Constants.YumikoColor,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"{strings.retrieved_from} openweathermap.org",
                            IconUrl = "https://images-ext-1.discordapp.net/external/3NnHdaMyO7CZtz9QO16w_yjJGG_HYUvGvkIleOZe1VY/http/openweathermap.org/img/w/03d.png",
                        },
                    };

                    embed.AddField($":cloud: {strings.weather}", $"{clima?.UppercaseFirst()}", true);
                    embed.AddField($":sweat: {strings.humidity}", $"{humedad}%", true);
                    embed.AddField($":ocean: {strings.pressure}", $"{presion} hPa", true);
                    embed.AddField($":dash: {strings.wind_speed}", $"{viento} m/s", true);
                    embed.AddField($":thermometer: {strings.temperature}", $"{temperatura} °C", true);
                    embed.AddField($":thermometer_face: {strings.feels_like}", $"{sensasionTermica} °C", true);
                    embed.AddField($":high_brightness: {strings.feels_like}", $"{min} °C - {max} °C", true);
                    embed.AddField($":sunrise_over_mountains: {strings.sunrise}", $"<t:{sunrise}:t>", true);
                    embed.AddField($":city_sunset: {strings.sunset}", $"<t:{sunset}:t>", true);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = strings.error,
                        Description = string.Format(strings.location_not_found, localidad, pais),
                        Color = DiscordColor.Red,
                    }));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = strings.error,
                        Description = strings.unknown_error,
                        Color = DiscordColor.Red,
                    }));
                }
            }
        }

        [SlashCommand("cat", "Random kitten")]
        public async Task Cat(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var client = new RestClient(@"https://api.thecatapi.com/v1/images/search?limit=1");
            client.AddDefaultHeader("x-api-key", ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.TokenTheCatApi));
            var request = new RestRequest();

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful && response.Content != null)
            {
                var data = JsonConvert.DeserializeObject<dynamic>(response.Content);
                if (data != null)
                {
                    string urlImagen = data[0].url;
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = $"{strings.random_cat} (๑✪ᆺ✪๑)",
                        ImageUrl = urlImagen,
                        Color = Constants.YumikoColor,
                    }));
                    return;
                }
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = strings.unknown_error,
                Description = $"{strings.random_cat_error} :c",
                Color = DiscordColor.Red,
            }));
            await Common.GrabarLogErrorAsync(ctx, $"Unknown error in `/cat`\n\n`{response.StatusCode}: {response.StatusDescription}`");
        }

        [SlashCommand("dog", "Random dog")]
        public async Task Dog(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var client = new RestClient(@"https://api.thedogapi.com/v1/images/search?limit=1");
            client.AddDefaultHeader("x-api-key", ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.TokenTheDogApi));
            var request = new RestRequest();

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful && response.Content != null)
            {
                var data = JsonConvert.DeserializeObject<dynamic>(response.Content);
                if (data != null)
                {
                    string urlImagen = data[0].url;
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = $"{strings.random_dog} (❍ᴥ❍ʋ)",
                        ImageUrl = urlImagen,
                        Color = Constants.YumikoColor,
                    }));
                    return;
                }
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = strings.unknown_error,
                Description = $"{strings.random_dog_error} :c",
                Color = DiscordColor.Red,
            }));
            await Common.GrabarLogErrorAsync(ctx, $"Unknown error in `/dog`\n\n`{response.StatusCode}: {response.StatusDescription}`");
        }
    }
}
