using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Yumiko.Bot.Helpers;

public static class MemberNames
{
    /// <summary>
    /// Display name of each user in the guild, already escaped for markdown. Users who left the guild fall
    /// back to their global name, and deleted accounts to their id.
    /// </summary>
    public static async Task<Dictionary<ulong, string>> ResolveAsync(DiscordClient client, DiscordGuild guild, IEnumerable<ulong> userIds)
    {
        Dictionary<ulong, string> names = [];

        foreach (ulong userId in userIds.Distinct())
        {
            names[userId] = Formatter.Sanitize(await ResolveAsync(client, guild, userId));
        }

        return names;
    }

    private static async Task<string> ResolveAsync(DiscordClient client, DiscordGuild guild, ulong userId)
    {
        try
        {
            return (await guild.GetMemberAsync(userId)).DisplayName;
        }
        catch (NotFoundException)
        {
        }

        try
        {
            DiscordUser user = await client.GetUserAsync(userId);
            return user.GlobalName ?? user.Username;
        }
        catch (NotFoundException)
        {
            return $"{userId}";
        }
    }
}
