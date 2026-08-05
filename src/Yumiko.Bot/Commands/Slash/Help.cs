using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Entities;
using Yumiko.Application.Helpers;
using Yumiko.Bot.Commands.Framework;
using Yumiko.Bot.Commands.Framework.Attributes;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Extensions;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services;

namespace Yumiko.Bot.Commands.Slash;

[TestCommand]
public sealed class Help(
    ILocalizer localizer,
    DiscordBotService discordBotService,
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

        bool isLogGuild = ctx.Guild?.Id == config.LogGuildId;

        string description = Formatter.BlockCode(loc.Format(Keys.bot_about, ctx.Client.CurrentUser.Username)) + "\n";

        IEnumerable<IGrouping<string, Command>> categories = ctx.Extension.Commands.Values
            .Where(c => c.Name != "owner" || isLogGuild)
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

        if (!string.IsNullOrEmpty(ctx.Client.CurrentApplication.PrivacyPolicyUrl))
        {
            buttons.Add(new DiscordLinkButtonComponent(ctx.Client.CurrentApplication.PrivacyPolicyUrl, loc[Keys.privacy_policy]));
        }

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
    /// The category is the class the command is declared in, which is what groups <c>/trivia</c> and
    /// <c>/hangman</c> under Games without them being a command group.
    /// </summary>
    private static string Category(Command command) =>
        (command.Method?.DeclaringType ?? command.Subcommands.FirstOrDefault()?.Method?.DeclaringType)?.Name
            ?? command.Name.UppercaseFirst();

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
