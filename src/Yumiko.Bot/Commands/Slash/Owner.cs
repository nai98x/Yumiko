using System.ComponentModel;
using System.Globalization;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Hosting;
using Yumiko.Bot.Commands.Framework;
using Yumiko.Bot.Commands.Framework.Attributes;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Extensions;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services.State;
using Yumiko.Model.Entities.Anilist;
using Yumiko.Model.Interfaces;

namespace Yumiko.Bot.Commands.Slash;

[TestCommand]
[LogGuildOnly]
[Command("owner")]
[Description("Commands only available to Yumiko's owner")]
[RequireApplicationOwner]
public sealed class Owner(
    ILocalizer localizer,
    IAnilistClient anilist,
    CommandUsageState commandUsage,
    InteractivityExtension interactivity,
    TimeoutSettings timeouts,
    IHostApplicationLifetime lifetime)
{
    private const int GuildsPerPage = 10;

    [Command("test")]
    [Description("Testing command")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task TestAsync(SlashCommandContext ctx)
    {
        await ctx.DeferResponseAsync();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Test"));
    }

    [Command("guild")]
    [Description("Information about a server")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task GuildAsync(
        SlashCommandContext ctx,
        [Parameter("guild_id")] [Description("Server Id to see details")] string id)
    {
        Loc loc = ctx.Loc(localizer);

        await ctx.DeferResponseAsync();

        if (!ulong.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong guildId))
        {
            await ctx.EditResponseAsync(ErrorEmbed.Create(loc[Keys.error], loc.Format(Keys.id_not_valid, id)));
            return;
        }

        if (!ctx.Client.Guilds.TryGetValue(guildId, out DiscordGuild? guild))
        {
            await ctx.EditResponseAsync(ErrorEmbed.Create(loc[Keys.error], loc.Format(Keys.guild_with_id_not_found, id)));
            return;
        }

        await ctx.EditResponseAsync(new DiscordEmbedBuilder
        {
            Title = guild.Name,
            Description = Description(guild, loc),
            Color = DiscordColor.Green,
        });
    }

    [Command("guilds")]
    [Description("See Yumiko's servers")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task GuildsAsync(SlashCommandContext ctx)
    {
        Loc loc = ctx.Loc(localizer);

        await ctx.DeferResponseAsync();

        List<Page> pages =
        [
            .. ctx.Client.Guilds.Values
                .OrderBy(g => g.JoinedAt)
                .Chunk(GuildsPerPage)
                .Select(group => new Page
                {
                    Embed = new DiscordEmbedBuilder
                    {
                        Title = loc.Format(Keys.bot_guilds, ctx.Client.CurrentUser.Username),
                        Description = string.Join("\n\n", group.Select(g => $"{Formatter.Bold(g.Name)}\n{Description(g, loc)}")).NormalizeDescription(),
                        Color = YumikoColors.Primary,
                    },
                }),
        ];

        await ctx.DeleteResponseAsync();

        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(timeouts.General));
        await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages, token: cts.Token);
    }

    [Command("deleteguild")]
    [Description("Yumiko leaves a server")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task DeleteGuildAsync(
        SlashCommandContext ctx,
        [Parameter("id")] [Description("Server Id to leave")] string id)
    {
        Loc loc = ctx.Loc(localizer);

        await ctx.DeferResponseAsync();

        if (!ulong.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong guildId))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(loc.Format(Keys.id_not_valid, id)));
            return;
        }

        try
        {
            DiscordGuild guild = await ctx.Client.GetGuildAsync(guildId);
            string name = guild.Name;
            await guild.LeaveAsync();
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(loc.Format(Keys.bot_left_guild, name, guildId)));
        }
        catch
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(loc.Format(Keys.error_retrieving_guild_with_id, id)));
        }
    }

    [Command("ratelimits")]
    [Description("Shows the ratelimits")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task RatelimitsAsync(SlashCommandContext ctx)
    {
        await ctx.DeferResponseAsync();

        AnilistRateLimit rateLimit = await anilist.GetRateLimitAsync();

        await ctx.EditResponseAsync(new DiscordEmbedBuilder
        {
            Title = "Ratelimits",
            Description = $"{Formatter.Bold("AniList:")}\nLimit: {rateLimit.Limit}\nRemaining: {rateLimit.Remaining}",
            Color = YumikoColors.Primary,
        });
    }

    [Command("commands")]
    [Description("Shows the commands used since startup")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task CommandsUsedAsync(SlashCommandContext ctx)
    {
        Loc loc = ctx.Loc(localizer);

        await ctx.DeferResponseAsync();

        IReadOnlyList<(string CommandName, int Uses)> uses = commandUsage.Snapshot();

        string description = uses.Count == 0
            ? loc[Keys.commands_not_used]
            : string.Join("\n", uses.Select(u => $"{Formatter.Bold(u.CommandName)} - {loc[Keys.uses]}: {u.Uses}"));

        await ctx.EditResponseAsync(new DiscordEmbedBuilder
        {
            Title = loc[Keys.commands_used],
            Description = description.NormalizeDescription(),
            Color = YumikoColors.Primary,
        });
    }

    [Command("logs")]
    [Description("Shows the lastest log file")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task LogsAsync(SlashCommandContext ctx)
    {
        Loc loc = ctx.Loc(localizer);

        await ctx.DeferResponseAsync();

        DirectoryInfo folder = new(Path.Combine(AppContext.BaseDirectory, "logs"));
        FileInfo? log = folder.Exists
            ? folder.GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault()
            : null;

        if (log is null)
        {
            await ctx.EditResponseAsync(ErrorEmbed.Create(loc[Keys.error], loc[Keys.no_logs_found]));
            return;
        }

        // Serilog keeps the file open: writing has to be shared to be able to read it.
        await using FileStream fs = File.Open(log.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent(loc[Keys.lastest_log_file])
            .AddFile(log.Name, fs));
    }

    [Command("poweroff")]
    [Description("Turn off the bot")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task PowerOffAsync(SlashCommandContext ctx)
    {
        Loc loc = ctx.Loc(localizer);

        await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
            .AsEphemeral()
            .WithContent(loc[Keys.shutting_down]));

        // Graceful shutdown: the host disconnects the client and flushes the Serilog buffers.
        lifetime.StopApplication();
    }

    private static string Description(DiscordGuild guild, Loc loc) =>
        $"  - {Formatter.Bold("Id")}: {guild.Id}\n" +
        $"  - {Formatter.Bold(loc[Keys.joined_date])}: {guild.JoinedAt}\n" +
        $"  - {Formatter.Bold(loc[Keys.member_count])}: {guild.MemberCount}";
}
