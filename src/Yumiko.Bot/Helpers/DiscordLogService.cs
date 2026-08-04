using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Yumiko.Bot.Services;

namespace Yumiko.Bot.Helpers;

/// <summary>
/// Escribe los errores y el alta/baja de guilds en los canales de log del guild propio.
/// </summary>
public sealed class DiscordLogService(DiscordBotService discordBotService, ILogger<DiscordLogService> logger)
{
    public async Task LogExceptionAsync(DiscordGuild? guild, DiscordChannel? channel, Exception ex, string context)
    {
        logger.LogError(ex, "Error en {Contexto}", context);

        await SendToErrorChannelAsync(new DiscordEmbedBuilder
        {
            Title = "Unknown error",
            Description = $"{context}: {ex.Message}\n{Formatter.BlockCode(ex.StackTrace ?? string.Empty)}".NormalizeDescription(),
            Color = DiscordColor.Red,
            Author = guild is null ? null : new DiscordEmbedBuilder.EmbedAuthor { IconUrl = guild.IconUrl, Name = guild.Name },
        }
        .AddField("Guild Id", guild is null ? "-" : $"{guild.Id}", true)
        .AddField("Channel Id", channel is null ? "-" : $"{channel.Id}", true)
        .AddField("Channel", channel is null ? "-" : $"#{channel.Name}"));
    }

    public Task LogErrorAsync(DiscordGuild? guild, DiscordChannel? channel, string description)
    {
        logger.LogError("{Descripcion}", description);

        return SendToErrorChannelAsync(new DiscordEmbedBuilder
        {
            Title = "Unknown error",
            Description = description.NormalizeDescription(),
            Color = DiscordColor.Red,
            Author = guild is null ? null : new DiscordEmbedBuilder.EmbedAuthor { IconUrl = guild.IconUrl, Name = guild.Name },
        }
        .AddField("Guild Id", guild is null ? "-" : $"{guild.Id}", true)
        .AddField("Channel Id", channel is null ? "-" : $"{channel.Id}", true));
    }

    public async Task LogGuildAsync(DiscordGuild guild, int guildCount, bool added)
    {
        if (!discordBotService.Initialized)
        {
            return;
        }

        string description =
            $"   **Id**: {guild.Id}\n" +
            $"   **Members**: {guild.MemberCount}\n";

        if (added)
        {
            description += $"   **Owner**: <@{guild.OwnerId}>\n";
        }

        description += $"\n   **Guild count**: {guildCount}";

        try
        {
            await discordBotService.LogChannelGuilds.SendMessageAsync(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = guild.IconUrl, Name = guild.Name },
                Title = added ? "Guild added" : "Guild removed",
                Description = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"{DateTimeOffset.Now}" },
                Color = added ? DiscordColor.Green : DiscordColor.Red,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo escribir en el canal de guilds");
        }
    }

    public void LogCommandExecuted(CommandContext ctx)
    {
        logger.LogInformation(
            "Comando ejecutado: /{Comando} | Guild: {Guild} | Usuario: {Usuario}",
            ctx.Command.FullName,
            ctx.Guild?.Name ?? "DM",
            ctx.User.Username);
    }

    private async Task SendToErrorChannelAsync(DiscordEmbedBuilder embed)
    {
        // Si el bot todavía no resolvió los canales, el log de Serilog ya dejó registro del error.
        if (!discordBotService.Initialized)
        {
            return;
        }

        try
        {
            await discordBotService.LogChannelErrors.SendMessageAsync(embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo escribir en el canal de errores");
        }
    }
}
