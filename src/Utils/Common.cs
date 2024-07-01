namespace Yumiko.Utils
{
    using DiscordBotsList.Api;
    using Google.Cloud.Firestore;
    using Google.Cloud.Firestore.V1;
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats.Png;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public static class Common
    {
        private static readonly GraphQLHttpClient GraphQlClient = new("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        /// <summary>
        /// Gets the Firestore client retrieved from the firebase.json file.
        /// </summary>
        /// <returns>The <see cref="FirestoreClient"/> client.</returns>
        public static FirestoreDb GetFirestoreClient()
        {
            string path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "res", "firebase.json");
            string jsonString = File.ReadAllText(path);

            if (jsonString != null)
            {
                var deserialized = JsonConvert.DeserializeObject(jsonString);
                if (deserialized != null)
                {
                    JObject? data = (JObject)deserialized;
                    var projectId = data.SelectToken("project_id")?.Value<string>();
                    if (projectId != null)
                    {
                        var builder = new FirestoreClientBuilder { JsonCredentials = jsonString };
                        return FirestoreDb.Create(projectId, builder.Build());
                    }
                }
            }

            throw new NullReferenceException("Something go wrong with firease.json");
        }

        /// <summary>
        /// Gets a random number between a minimum and a maximum.
        /// </summary>
        /// <param name="min">Minimum number.</param>
        /// <param name="max">Maximum number.</param>
        /// <returns>A random number generated.</returns>
        public static int GetRandomNumber(int min, int max)
        {
            Random rnd = new();

            if (min <= 0 && max <= 0)
            {
                return 0;
            }

            if (min + 1 == max)
            {
                return (rnd.Next(100) < 50) ? min : max;
            }

            return rnd.Next(minValue: min, maxValue: max + 1);
        }

        /// <summary>
        /// Converts a string to a <see cref="DiscordEmoji"/> object.
        /// </summary>
        /// <param name="text">String representation of a <see cref="DiscordEmoji"/>.</param>
        /// <returns>A <see cref="DiscordEmoji"/> object.</returns>
        public static DiscordEmoji? ToEmoji(string text)
        {
            text = text.Trim();
            var match = Regex.Match(text, @"^<?a?:?([a-zA-Z0-9_]+):([0-9]+)>?$");
            if (!match.Success)
            {
                return DiscordEmoji.TryFromUnicode(text, out var emoji) ? emoji : null;
            }

            string json = $"{{\"name\":\"{match.Groups[1].Value}\", \"id\":{match.Groups[2].Value}," +
                $"\"animated\":{text.StartsWith("<a:").ToString().ToLower()}, \"require_colons\":true, \"available\":true}}";
            return JsonConvert.DeserializeObject<DiscordEmoji>(json);
        }

        /// <summary>
        /// Remove non alphanumeric characters from a <see cref="string"/>.
        /// </summary>
        /// <param name="str">Interaction Context.</param>
        /// <returns>The <see cref="string"/> without non-aplhanumeric characters</returns>
        public static string QuitarCaracteresEspeciales(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return Regex.Replace(str, @"[^a-zA-Z0-9]+", " ").Trim();
            }

            return string.Empty;
        }

        /// <summary>
        /// Removes HTML from AniList queries.
        /// </summary>
        /// <param name="texto">A <see cref="string"/> from AniList.</param>
        /// <returns><see cref="string"/> without HTML tags.</returns>
        public static string LimpiarTexto(string texto)
        {
            if (texto != null)
            {
                texto = texto.Replace("<br>", string.Empty);
                texto = texto.Replace("<Br>", string.Empty);
                texto = texto.Replace("<bR>", string.Empty);
                texto = texto.Replace("<BR>", string.Empty);
                texto = texto.Replace("<i>", "*");
                texto = texto.Replace("<I>", "*");
                texto = texto.Replace("</i>", "*");
                texto = texto.Replace("</I>", "*");
                texto = texto.Replace("~!", "||");
                texto = texto.Replace("!~", "||");
                texto = texto.Replace("__", "**");
                texto = texto.Replace("<b>", "**");
                texto = texto.Replace("<B>", "**");
                texto = texto.Replace("</b>", "**");
                texto = texto.Replace("</B>", "**");
            }
            else
            {
                texto = string.Empty;
            }

            return texto;
        }

        /// <summary>
        /// Checks if the current user has alredy voted in Top.GG.
        /// If not, a <see cref="DiscordMessage"/> will be sent.
        /// </summary>
        /// <param name="ctx">Interaction Context.</param>
        /// <param name="topggToken">Token used for Top.GG API.</param>
        public static async Task ChequearVotoTopGGAsync(InteractionContext ctx, string topggToken)
        {
            if (Program.TopggEnabled && !Program.Debug)
            {
                AuthDiscordBotListApi DblApi = new(ctx.Client.CurrentApplication.Id, topggToken);
                bool hasVoted = await DblApi.HasVoted(ctx.User.Id);

                if (!hasVoted)
                {
                    string url = $"https://top.gg/bot/{ctx.Client.CurrentUser.Id}/vote";
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                    {
                        Title = translations.vote_me_on_topgg,
                        Description = string.Format(translations.vote_me_on_topgg_desc, url),
                        Color = Constants.YumikoColor,
                        Footer = new()
                        {
                            Text = translations.message_will_not_be_triggered
                        }
                    }));
                }
            }
        }

        /// <summary>
        /// Checks monthly votes in Top.GG for the bot, if enabled.
        /// </summary>
        /// <param name="ctx">Interaction Context.</param>
        /// <param name="topggToken">Token used for Top.GG API.</param>
        /// <returns>Monthly Top.GG vote count.</returns>
        public static async Task<int> CheckTopGGVotesCountAsync(InteractionContext ctx, string topggToken)
        {
            if (Program.TopggEnabled && !Program.Debug)
            {
                AuthDiscordBotListApi DblApi = new(ctx.Client.CurrentApplication.Id, topggToken);
                var voters = await DblApi.GetVotersAsync();

                return voters.Count;
            }

            throw new NotSupportedException("Could not retreieve vote count. Reason: Top.GG has not been enabled properly");
        }

        /// <summary>
        /// Updates the guild and shard count for Top.GG.
        /// </summary>
        /// <param name="applicationId">Application ID from Discord.</param>
        /// <param name="topggToken">Token used for Top.GG API.</param>
        public static async Task UpdateStatsTopGGAsync(ulong applicationId, string topggToken)
        {
            AuthDiscordBotListApi DblApi = new(applicationId, topggToken);
            var totalGuilds = Program.DiscordShardedClient.GuildCount();
            var totalShards = Program.DiscordShardedClient.ShardCount();

            await DblApi.UpdateStats(guildCount: totalGuilds, shardCount: totalShards);
        }

        /// <summary>
        /// A user chooses an option from a list using a <see cref="DiscordSelectComponent"/>. Up to 25 options.
        /// </summary>
        /// <param name="ctx">Interaction Context.</param>
        /// <param name="timeoutGeneral">Time that the user have to choose an option.</param>
        /// <param name="opciones">List of options to choose.</param>
        /// <returns>Index of the array for the choosen option.</returns>
        public static async Task<int> GetElegidoAsync(InteractionContext ctx, double timeoutGeneral, List<TitleDescription> opciones)
        {
            int cantidadOpciones = opciones.Count;
            if (cantidadOpciones == 1)
            {
                return 1;
            }

            var interactivity = ctx.Client.GetInteractivity();
            List<DiscordSelectComponentOption> options = new();
            string customId = "dropdownGetElegido";

            int i = 0;
            opciones.ForEach(opc =>
            {
                if (i < 25 && opc.Title != null)
                {
                    i++;
                    options.Add(new DiscordSelectComponentOption(opc.Title.NormalizeButton(), $"{i}", opc.Description));
                }
            });

            var dropdown = new DiscordSelectComponent(customId, translations.select_an_option, options);

            var embed = new DiscordEmbedBuilder
            {
                Color = Constants.YumikoColor,
                Title = translations.choose_an_option,
            };

            DiscordMessage elegirMsg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(dropdown).AddEmbed(embed));

            var msgElegirInter = await interactivity.WaitForSelectAsync(elegirMsg, ctx.User, customId, TimeSpan.FromSeconds(timeoutGeneral));

            if (!msgElegirInter.TimedOut)
            {
                var resultElegir = msgElegirInter.Result;
                return int.Parse(resultElegir.Values[0]);
            }

            return -1;
        }

        /// <summary>
        /// A user chooses yes or no using buttons.
        /// </summary>
        /// <param name="ctx">Interaction Context.</param>
        /// <param name="timeoutGeneral">Time that the user have to choose an option.</param>
        /// <param name="interactivity">The interactivity extension.</param>
        /// <param name="title">Title of the embed.</param>
        /// <param name="description">Description of the embed.</param>
        /// <returns>True if the user has answered yes, false if no or timeout.</returns>
        public static async Task<bool> GetYesNoInteractivityAsync(InteractionContext ctx, double timeoutGeneral, InteractivityExtension interactivity, string title, string description)
        {
            DiscordButtonComponent yesButton = new(ButtonStyle.Success, "true", translations.yes);
            DiscordButtonComponent noButton = new(ButtonStyle.Danger, "false", translations.no);

            var msgBuilder = new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = title,
                Description = description,
            });

            msgBuilder.AddComponents(yesButton, noButton);

            DiscordMessage chooseMsg = await ctx.FollowUpAsync(msgBuilder);
            var msgElegirInter = await interactivity.WaitForButtonAsync(chooseMsg, ctx.User, TimeSpan.FromSeconds(timeoutGeneral));
            await ctx.DeleteFollowupAsync(chooseMsg.Id);
            if (!msgElegirInter.TimedOut)
            {
                return bool.Parse(msgElegirInter.Result.Id);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a random character from AniList.
        /// </summary>
        /// <param name="ctx">Interaction Context.</param>
        /// <param name="pag">AniList page of characters query, ordered by popularity.</param>
        /// <returns>A <see cref="CharacterOld"/> object.</returns>
        public static async Task<CharacterOld?> GetRandomCharacterAsync(InteractionContext ctx, int pag)
        {
            string titleMedia = string.Empty, siteUrlMedia = string.Empty;
            string query = "query($pagina: Int){" +
                        "   Page(page: $pagina, perPage: 1){" +
                        "       characters(sort: FAVOURITES_DESC){" +
                        "           name{" +
                        "               full" +
                        "           }," +
                        "           image{" +
                        "               large" +
                        "           }" +
                        "           siteUrl," +
                        "           favourites," +
                        "           media(sort: POPULARITY_DESC, perPage: 1){" +
                        "               nodes{" +
                        "                   title{" +
                        "                       romaji" +
                        "                   }," +
                        "                   siteUrl" +
                        "               }" +
                        "           }" +
                        "       }" +
                        "   }" +
                        "}";
            var request = new GraphQLRequest
            {
                Query = query,
                Variables = new
                {
                    pagina = pag,
                },
            };
            try
            {
                var data = await GraphQlClient.SendQueryAsync<dynamic>(request);
                foreach (var x in data.Data.Page.characters)
                {
                    string name = x.name.full;
                    string imageUrl = x.image.large;
                    string siteUrl = x.siteUrl;
                    int favoritos = x.favourites;
                    foreach (var m in x.media.nodes)
                    {
                        titleMedia = m.title.romaji;
                        siteUrlMedia = m.siteUrl;
                    }

                    return new CharacterOld()
                    {
                        NameFull = name,
                        Image = imageUrl,
                        SiteUrl = siteUrl,
                        Favoritos = favoritos,
                        AnimePrincipal = new Anime()
                        {
                            TitleRomaji = titleMedia,
                            SiteUrl = siteUrlMedia,
                        },
                    };
                }
            }
            catch (Exception ex)
            {
                await GrabarLogErrorAsync(ctx.Guild, ctx.Channel, $"Unknown error in GetRandomCharacter");
                _ = ex.Message switch
                {
                    _ => await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{translations.unknown_error}: {ex.Message}")),
                };
                throw;
            }

            return null;
        }

        /// <summary>
        /// Gets a random media from AniList.
        /// </summary>
        /// <param name="ctx">Interaction Context.</param>
        /// <param name="pag">AniList page of media query, ordered by popularity.</param>
        /// <param name="type">Anime or Manga.</param>
        /// <returns>A <see cref="Anime"/> object.</returns>
        public static async Task<Anime?> GetRandomMediaAsync(InteractionContext ctx, int pag, MediaType type)
        {
            string query = "query($pagina: Int){" +
                        "   Page(page: $pagina, perPage: 1){" +
                        "       media(sort: FAVOURITES_DESC, isAdult: false, type:" + type.GetName() + "){" +
                        "           title{" +
                        "               romaji," +
                        "               english" +
                        "           }," +
                        "           coverImage{" +
                        "               large" +
                        "           }," +
                        "           siteUrl," +
                        "           favourites" +
                        "       }" +
                        "   }" +
                        "}";
            var request = new GraphQLRequest
            {
                Query = query,
                Variables = new
                {
                    pagina = pag,
                },
            };
            try
            {
                var data = await GraphQlClient.SendQueryAsync<dynamic>(request);
                foreach (var x in data.Data.Page.media)
                {
                    string titleRomaji = x.title.romaji;
                    string titleEnglish = x.title.english;
                    string imageUrl = x.coverImage.large;
                    string siteUrl = x.siteUrl;
                    int favoritos = x.favourites;
                    return new Anime()
                    {
                        TitleRomaji = titleRomaji,
                        TitleEnglish = titleEnglish,
                        Image = imageUrl,
                        SiteUrl = siteUrl,
                        Favoritos = favoritos,
                    };
                }
            }
            catch (Exception ex)
            {
                await GrabarLogErrorAsync(ctx.Guild, ctx.Channel, $"Unknown error in GetRandomMedia");
                _ = ex.Message switch
                {
                    _ => await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{translations.unknown_error}: {ex.Message}")),
                };
                throw;
            }

            return null;
        }

        /// <summary>
        /// Logs an unknown error to the <see cref="DiscordChannel"/> designed to logs.
        /// </summary>
        /// <param name="guild">Guild where the error ocurred.</param>
        /// <param name="channel">Channel where the error ocurred.</param>
        /// <param name="description">Description of the error. Typically contains the Stack Trace.</param>
        public static async Task GrabarLogErrorAsync(DiscordGuild guild, DiscordChannel channel, string description)
        {
            await Program.LogChannelErrors.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Unknown error",
                Description = description,
                Color = DiscordColor.Red,
                Author = new()
                {
                    IconUrl = guild.IconUrl,
                    Name = guild.Name,
                },
            }.AddField("Guild Id", $"{guild.Id}", true)
            .AddField("Channel Id", $"{channel.Id}", true)
            .AddField("Channel", $"#{channel.Name}", false));
        }

        /// <summary>
        /// Merges two images from links.
        /// </summary>
        /// <param name="link1">Link for the first image.</param>
        /// <param name="link2">Link for the second image.</param>
        /// <param name="x">Width of the merged image.</param>
        /// <param name="y">Height of the merged image.</param>
        /// <returns>The merged image.</returns>
        public static async Task<byte[]> MergeImageAsync(string link1, string link2, int x, int y)
        {
            var client = new HttpClient();
            var bytes1 = await client.GetByteArrayAsync(link1);
            var bytes2 = await client.GetByteArrayAsync(link2);

            using var memoryStream = new MemoryStream();
            using Image<Rgba32> img1 = Image.Load<Rgba32>(bytes1); // load up source images
            using Image<Rgba32> img2 = Image.Load<Rgba32>(bytes2);

            using var outputImage = new Image<Rgba32>(x, y); // create output image of the correct dimensions

            img1.Mutate(o => o.Resize(new Size(x / 2, y)));
            img2.Mutate(o => o.Resize(new Size(x / 2, y)));

            // take the 2 source images and draw them onto the image
            outputImage.Mutate(o => o
                .DrawImage(img1, new Point(0, 0), 1f) // draw the first one top left
                .DrawImage(img2, new Point(x / 2, 0), 1f)); // draw the second next to it

            // This saves to the memoryStream with encoder
            outputImage.Save(memoryStream, new PngEncoder());
            memoryStream.Position = 0; // The position needs to be reset.

            // return byte[]
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Overlaps two images into one.
        /// </summary>
        /// <param name="image1">The first image.</param>
        /// <param name="image2">The second image.</param>
        /// <param name="x">Width of the merged image.</param>
        /// <param name="y">Height of the merged image.</param>
        /// <returns>The overlapped image.</returns>
        public static byte[] OverlapImage(byte[] image1, byte[] image2, int x, int y)
        {
            using var memoryStream = new MemoryStream();
            using var outputImage = new Image<Rgba32>(x, y);
            using Image<Rgba32> img1 = Image.Load<Rgba32>(image1);
            using Image<Rgba32> img2 = Image.Load<Rgba32>(image2);

            outputImage.Mutate(o => o
                .DrawImage(img1, new Point(0, 0), 1f)
                .DrawImage(img2, new Point(0, 0), 1f));

            outputImage.Save(memoryStream, new PngEncoder());
            memoryStream.Position = 0;

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Gets the newest created file in a Directory.
        /// </summary>
        /// <param name="directory">The directory to search.</param>
        /// <returns>The newest file.</returns>
        public static FileInfo? GetNewestFile(DirectoryInfo directory)
        {
            return directory.GetFiles()
                .Union(directory.GetDirectories().Select(d => GetNewestFile(d)))
                .OrderByDescending(f => (f == null ? DateTime.MinValue : f.LastWriteTime))
                .FirstOrDefault();
        }

        /// <summary>
        /// Makes an NSFW warning <see cref="DiscordEmbed"/>.
        /// </summary>
        public static DiscordEmbedBuilder NsfwWarning { get; private set; } = new DiscordEmbedBuilder
        {
            Title = translations.nsfw_required,
            Description = translations.use_command_in_nsfw_channel,
            Color = DiscordColor.Red,
        };

        /// <summary>
        /// Makes an resource not found <see cref="DiscordEmbed"/>.
        /// </summary>
        public static DiscordEmbedBuilder ResourceNotFound(string resource)
        {
            return new DiscordEmbedBuilder
            {
                Title = string.Format(translations.not_found, resource),
                Description = translations.resource_not_found,
                Color = DiscordColor.Red,
            };
        }

        public static Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(ctx.Interaction.Locale!);

            if (!Singleton.GetInstance().IsBotReady())
            {
                _ = Task.Run(async () =>
                {
                    await ctx.CreateResponseAsync(new DiscordEmbedBuilder()
                    .WithTitle(translations.error)
                    .WithDescription(translations.bot_not_ready)
                    .WithColor(DiscordColor.Red)
                    );
                });

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public static Task<bool> BeforeContextMenuExecutionAsync(ContextMenuContext ctx)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(ctx.Interaction.Locale!);

            if (!Singleton.GetInstance().IsBotReady())
            {
                _ = Task.Run(async () =>
                {
                    await ctx.CreateResponseAsync(new DiscordEmbedBuilder()
                    .WithTitle(translations.error)
                    .WithDescription(translations.bot_not_ready)
                    .WithColor(DiscordColor.Red)
                    );
                });

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
