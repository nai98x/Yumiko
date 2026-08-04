using DSharpPlus.Entities;
using Yumiko.Bot.Localization;

namespace Yumiko.Bot.Helpers;

public static class ErrorEmbed
{
    public static DiscordEmbedBuilder Create(string title, string description) => new()
    {
        Title = title,
        Description = description,
        Color = DiscordColor.Red,
    };

    public static DiscordEmbedBuilder Unknown(Loc loc) =>
        Create(loc[Keys.error], loc[Keys.unknown_error]);

    public static DiscordEmbedBuilder Forbidden(Loc loc, string? detail = null) =>
        Create(loc[Keys.access_denied], detail ?? loc[Keys.only_bot_owner]);

    public static DiscordEmbedBuilder NotFound(Loc loc, string resource) =>
        Create(loc.Format(Keys.not_found, resource), loc[Keys.resource_not_found]);

    public static DiscordEmbedBuilder NsfwRequired(Loc loc) =>
        Create(loc[Keys.nsfw_required], loc[Keys.use_command_in_nsfw_channel]);
}
