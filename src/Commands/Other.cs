namespace Yumiko.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using RestSharp;
    using System.Threading.Tasks;
    using System.Web;
    using Yumiko.Providers;
    using Yumiko.Utils;

    public class Other : ApplicationCommandModule
    {
        public IConfigurationRoot Configuration { private get; set; } = null!;

        [SlashCommand("ping", "Shows Yumiko´s ping")]
        public async Task Ping(InteractionContext ctx)
        {
            Common.GetFirestoreClient(Configuration);
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
            var client = new RestClient(baseUrl + $"?q={HttpUtility.UrlEncode(localidad)},{pais}&appid={Configuration.GetValue<string>("tokens:openweathermap")}&lang=en&units=metric");
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
            client.AddDefaultHeader("x-api-key", Configuration.GetValue<string>("tokens:thecatapi"));
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
            client.AddDefaultHeader("x-api-key", Configuration.GetValue<string>("tokens:thedogapi"));
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
    }
}
