using DSharpPlus.Entities;

namespace Yumiko.Bot.Extensions;

public static class DiscordMemberExtensions
{
    public static string PreferredAvatarUrl(this DiscordMember member) =>
        member.GuildAvatarUrl ?? member.AvatarUrl;
}
