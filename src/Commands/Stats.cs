namespace Yumiko.Commands
{
    using Humanizer;
    using Humanizer.Localisation;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Not with D#+ Command classes")]
    [SlashCommandGroup("stats", "Statistics from different games")]
    public class Stats : ApplicationCommandModule
    {
        public IConfigurationRoot Configuration { private get; set; } = null!;

        public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
        {
            return Common.BeforeSlashExecutionAsync(ctx);
        }

        public override Task<bool> BeforeContextMenuExecutionAsync(ContextMenuContext ctx)
        {
            return Common.BeforeContextMenuExecutionAsync(ctx);
        }

        [SlashCommand("user", "Shows the statistics of all games of a user")]
        [NameLocalization(Localization.Spanish, "usuario")]
        [DescriptionLocalization(Localization.Spanish, "Muestra las estadisticas de todos los juegos de un usuario")]
        [SlashRequirePermissions(Permissions.SendMessages)]
        public async Task User(InteractionContext ctx, [Option("User", "The user's stats to retrieve")] DiscordUser? user = null)
        {
            await ctx.DeferAsync();
            user ??= ctx.User;
            var builder1 = await GameServices.GetUserTriviaStats(ctx, user);
            var builder2 = await GameServices.GetUserTriviaGenreStats(ctx, user.Id);
            var builder3 = await GameServices.GetUserHoLStats(ctx, user.Id);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder1).AddEmbed(builder2).AddEmbed(builder3));
            await Common.ChequearVotoTopGGAsync(ctx, ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.TokenTopgg));
        }

        [SlashCommand("trivia", "Shows the statistics of the trivia game")]
        [DescriptionLocalization(Localization.Spanish, "Muestra las estadísticas del juego trivia")]
        [SlashRequirePermissions(Permissions.SendMessages)]
        public async Task Trivia(InteractionContext ctx, [Option("Game", "The gamemode you want to see the stats")] Gamemode gamemode)
        {
            await ctx.DeferAsync();
            var interactivity = ctx.Client.GetInteractivity();
            DiscordEmbedBuilder builder;

            if (gamemode != Gamemode.Genres)
            {
                builder = await GameServices.GetEstadisticasAsync(ctx, gamemode);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                await Common.ChequearVotoTopGGAsync(ctx, ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.TokenTopgg));
            }
            else
            {
                var respuesta = await GameServices.ElegirGeneroAsync(ctx, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), interactivity);
                if (respuesta.Ok && respuesta.Genre != null)
                {
                    builder = await GameServices.GetEstadisticasGeneroAsync(ctx, respuesta.Genre);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                    await Common.ChequearVotoTopGGAsync(ctx, ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.TokenTopgg));
                }
                else
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = translations.error,
                        Description = translations.no_genre_selected,
                        Color = DiscordColor.Red,
                    }));
                }
            }
        }

        [SlashCommand("higherorlower", "Shows the statistics of the Higher or Lower game")]
        [DescriptionLocalization(Localization.Spanish, "Muestra las estadísticas del juego Higher or Lower")]
        [SlashRequirePermissions(Permissions.SendMessages)]
        public async Task HigherOrLower(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            var builder = await GameServices.GetEstadisticasHoLAsync(ctx);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
            await Common.ChequearVotoTopGGAsync(ctx, ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.TokenTopgg));
        }

        [SlashCommand("delete", "Deletes user statistics on the server")]
        [NameLocalization(Localization.Spanish, "eliminar")]
        [DescriptionLocalization(Localization.Spanish, "Elimina tus estadísticas del servidor")]
        public async Task Delete(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            var interactivity = ctx.Client.GetInteractivity();

            string titulo = translations.confirm_delete_stats;
            string opciones = $"**{translations.action_cannont_be_undone}**";
            bool confirmar = await Common.GetYesNoInteractivityAsync(ctx, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), interactivity, titulo, opciones);
            if (confirmar)
            {
                await GameServices.EliminarEstadisticasAsync(ctx);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(translations.delete_stats_done));
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(translations.delete_stats_cancelled));
            }
        }

        [SlashCommand("bot", "Shows Yumiko's information and stats")]
        [DescriptionLocalization(Localization.Spanish, "Muestra información y estadísticas del bot")]
        public async Task Information(InteractionContext ctx)
        {
            await ctx.DeferAsync();

            GC.Collect(2, GCCollectionMode.Forced, true, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(2, GCCollectionMode.Forced, true, true);

            var heapMemory = $"{GC.GetTotalMemory(true) / 1024 / 1024:n0} MB";

            var embed = new DiscordEmbedBuilder()
            {
                Title = string.Format(translations.bot_stats, ctx.Client.CurrentUser.Username),
                Color = Constants.YumikoColor
            };

            embed.AddField(translations.library, $"DSharpPlus {ctx.Client.VersionString}", true);
            embed.AddField(translations.memory_usage, heapMemory, true);
            embed.AddField(translations.latency, $"{ctx.Client.Ping} ms", true);
            embed.AddField(translations.total_shards, $"{Program.DiscordShardedClient.ShardCount()}", true);
            embed.AddField(translations.total_guilds, $"{Program.DiscordShardedClient.GuildCount()}", true);
            embed.AddField(translations.total_users, $"{Program.DiscordShardedClient.UserCount()}", true);
            embed.AddField(translations.uptime, $"{Program.Stopwatch.Elapsed.Humanize(2, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day, culture: new CultureInfo(ctx.Interaction.Locale!))}", true);

            if (Program.TopggEnabled && !Program.Debug)
            {
                embed.AddField(translations.vote_count, $"{await Common.CheckTopGGVotesCountAsync(ctx, ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.TokenTopgg))}", true);
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
    }
}
