using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Localization;
using Yumiko.Model.Entities;

namespace Yumiko.Bot.Helpers;

/// <summary>
/// Los dos diálogos que usan varios comandos: elegir uno de N resultados y confirmar sí/no.
/// </summary>
public sealed class DiscordInteractivity(InteractivityExtension interactivity, TimeoutSettings timeouts)
{
    private const string SelectCustomId = "dropdownGetElegido";
    private const int MaxOptions = 25;

    /// <summary>
    /// Muestra un desplegable con las opciones y devuelve el índice elegido (base 0), o <c>null</c> si
    /// venció el tiempo. Con una sola opción no pregunta nada.
    /// </summary>
    public async Task<int?> ChooseAsync(SlashCommandContext ctx, IReadOnlyList<TitleDescription> options, Loc loc)
    {
        if (options.Count == 0)
        {
            return null;
        }

        if (options.Count == 1)
        {
            return 0;
        }

        List<DiscordSelectComponentOption> items =
        [
            .. options
                .Select((option, index) => (option, index))
                .Where(x => !string.IsNullOrWhiteSpace(x.option.Title))
                .Take(MaxOptions)
                .Select(x => new DiscordSelectComponentOption(
                    x.option.Title!.NormalizeButton(),
                    $"{x.index}",
                    x.option.Description?.NormalizeSelectMenuOption())),
        ];

        DiscordMessage message = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(new DiscordEmbedBuilder
            {
                Color = YumikoColors.Primary,
                Title = loc[Keys.choose_an_option],
            })
            .AddActionRowComponent(new DiscordSelectComponent(SelectCustomId, loc[Keys.select_an_option], items)));

        InteractivityResult<ComponentInteractionCreatedEventArgs> result =
            await interactivity.WaitForSelectAsync(message, ctx.User, SelectCustomId, TimeSpan.FromSeconds(timeouts.General));

        return result.TimedOut ? null : int.Parse(result.Result.Values[0]);
    }

    /// <summary>Confirmación con botones. Un timeout cuenta como "no".</summary>
    public async Task<bool> ConfirmAsync(SlashCommandContext ctx, string title, string description, Loc loc)
    {
        DiscordMessage message = await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder { Title = title, Description = description })
            .AddActionRowComponent(
                new DiscordButtonComponent(DiscordButtonStyle.Success, "true", loc[Keys.yes]),
                new DiscordButtonComponent(DiscordButtonStyle.Danger, "false", loc[Keys.no])));

        InteractivityResult<ComponentInteractionCreatedEventArgs> result =
            await interactivity.WaitForButtonAsync(message, ctx.User, TimeSpan.FromSeconds(timeouts.General));

        await ctx.DeleteFollowupAsync(message.Id);

        return !result.TimedOut && bool.Parse(result.Result.Id);
    }
}
