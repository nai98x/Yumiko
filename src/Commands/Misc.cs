namespace Yumiko.Commands
{
    using Humanizer;
    using Humanizer.Localisation;
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
            var client = new RestClient(baseUrl + $"?q={HttpUtility.UrlEncode(localidad)},{pais}&appid={ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.TokenOpenWeatherMap)}&lang=en&units=metric");
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
                        Title = $"Weather in {localidadNombre}",
                        Url = localidadUrl,
                        Color = Constants.YumikoColor,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = "Retrieved from openweathermap.org",
                            IconUrl = "https://images-ext-1.discordapp.net/external/3NnHdaMyO7CZtz9QO16w_yjJGG_HYUvGvkIleOZe1VY/http/openweathermap.org/img/w/03d.png",
                        },
                    };

                    embed.AddField(":cloud: Weather", $"{clima?.UppercaseFirst()}", true);
                    embed.AddField(":sweat: Humidity", $"{humedad}%", true);
                    embed.AddField(":ocean: Pressure", $"{presion} hPa", true);
                    embed.AddField(":dash: Wind speed", $"{viento} m/s", true);
                    embed.AddField(":thermometer: Temperature", $"{temperatura} °C", true);
                    embed.AddField(":thermometer_face: Feels like", $"{sensasionTermica} °C", true);
                    embed.AddField(":high_brightness: Min/Max", $"{min} °C - {max} °C", true);
                    embed.AddField(":sunrise_over_mountains: Sunrise", $"<t:{sunrise}:t>", true);
                    embed.AddField(":city_sunset: Sunset", $"<t:{sunset}:t>", true);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = "Error",
                        Description = $"Location `{localidad}, {pais}` not found",
                        Color = DiscordColor.Red,
                    }));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = "Error",
                        Description = "Unknown error",
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
                        Title = "Random kitten (๑✪ᆺ✪๑)",
                        ImageUrl = urlImagen,
                        Color = Constants.YumikoColor,
                    }));
                    return;
                }
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = "Unknown error",
                Description = "There was an error trying to get the kitten :c",
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
                        Title = "Random dog (❍ᴥ❍ʋ)",
                        ImageUrl = urlImagen,
                        Color = Constants.YumikoColor,
                    }));
                    return;
                }
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = "Unknown error",
                Description = "There was an error trying to get the dog :c",
                Color = DiscordColor.Red,
            }));
            await Common.GrabarLogErrorAsync(ctx, $"Unknown error in `/dog`\n\n`{response.StatusCode}: {response.StatusDescription}`");
        }

        [SlashCommand("info", "Shows Yumiko's information and stats")]
        public async Task Information(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var embed = new DiscordEmbedBuilder()
            {
                Title = "Information",
                Color = Constants.YumikoColor
            };

            embed.AddField("Author", ctx.Client.CurrentApplication.Owners.First().FullName(), true);
            embed.AddField("Library", $"DSharpPlus {ctx.Client.VersionString}", true);
            embed.AddField("Memory", $"{GC.GetTotalMemory(true) / 1024 / 1024:n0} MB", true);
            embed.AddField("Latency", $"{ctx.Client.Ping} ms", true);
            embed.AddField("Current shard", $"{ctx.Client.ShardId}", true);
            embed.AddField("Guilds", $"{ctx.Client.Guilds.Count}", true);
            embed.AddField("Uptime", $"{Program.Stopwatch.Elapsed.Humanize(2, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day, culture: new CultureInfo("en-US"))}", true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
    }
}
