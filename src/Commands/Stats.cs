namespace Yumiko.Commands
{
    using System.Threading.Tasks;

    [SlashCommandGroup("stats", "Statistics from different games")]
    public class Stats : ApplicationCommandModule
    {
        public IConfigurationRoot Configuration { private get; set; } = null!;

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
                        Title = "Error",
                        Description = "No genre has been selected",
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

            string titulo = "Confirm if you really want to delete your stats";
            string opciones = $"**This action cannont be undone**";
            bool confirmar = await Common.GetYesNoInteractivityAsync(context, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), interactivity, titulo, opciones);
            if (confirmar)
            {
                await GameServices.EliminarEstadisticasAsync(context);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"You have deleted all your statistics on this guild"));
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"You have chosen not to delete their statistics"));
            }
        }
    }
}
