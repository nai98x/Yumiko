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
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(ctx.Interaction.Locale!);
            return Task.FromResult(true);
        }

        [SlashCommand("trivia", "Shows the statistics of the trivia game")]
        [SlashRequirePermissions(Permissions.SendMessages)]
        public async Task Trivia(InteractionContext ctx, [Option("Game", "The gamemode you want to see the stats")] Gamemode gamemode)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = ctx;
            var interactivity = ctx.Client.GetInteractivity();
            DiscordEmbedBuilder builder;

            if (gamemode != Gamemode.Genres)
            {
                builder = await GameServices.GetEstadisticasAsync(context, gamemode);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                await Common.ChequearVotoTopGGAsync(ctx, ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.TokenTopgg));
            }
            else
            {    
                var respuesta = await GameServices.ElegirGeneroAsync(ctx, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), interactivity);
                if (respuesta.Ok && respuesta.Genre != null)
                {
                    builder = await GameServices.GetEstadisticasGeneroAsync(context, respuesta.Genre);
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
        [SlashRequirePermissions(Permissions.SendMessages)]
        public async Task HigherOrLower(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var builder = await GameServices.GetEstadisticasHoLAsync(ctx);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
            await Common.ChequearVotoTopGGAsync(ctx, ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.TokenTopgg));
        }

        [SlashCommand("delete", "Deletes user statistics on the guild")]
        public async Task Delete(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = ctx;
            var interactivity = ctx.Client.GetInteractivity();

            string titulo = translations.confirm_delete_stats;
            string opciones = $"**{translations.action_cannont_be_undone}**";
            bool confirmar = await Common.GetYesNoInteractivityAsync(context, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), interactivity, titulo, opciones);
            if (confirmar)
            {
                await GameServices.EliminarEstadisticasAsync(context);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(translations.delete_stats_done));
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(translations.delete_stats_cancelled));
            }
        }

        [SlashCommand("bot", "Shows Yumiko's information and stats")]
        public async Task Information(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

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
            embed.AddField(translations.total_shards, $"{Program.DiscordShardedClient.ShardClients.Count}", true);
            embed.AddField(translations.total_guilds, $"{Program.DiscordShardedClient.ShardClients.Values.Sum(x => x.Guilds.Count)}", true);
            embed.AddField(translations.total_users, $"{Program.DiscordShardedClient.ShardClients.Values.Sum(x => x.Guilds.Sum(y => y.Value.MemberCount))}", true);
            embed.AddField(translations.uptime, $"{Program.Stopwatch.Elapsed.Humanize(2, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day, culture: new CultureInfo(ctx.Interaction.Locale!))}", true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
    }
}
