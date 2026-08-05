using System.Reflection;
using DSharpPlus.Commands;
using Yumiko.Bot.Commands.Framework.Attributes;

namespace Yumiko.Bot.Extensions;

public static class CommandsExtensionExtensions
{
    private const string SlashCommandsNamespace = "Yumiko.Bot.Commands.Slash";
    private const string ContextMenuCommandsNamespace = "Yumiko.Bot.Commands.ContextMenu";

    /// <summary>
    /// Discovers by reflection the slash command classes of the matching namespace. In RELEASE
    /// they are registered globally, except the ones marked with <see cref="LogGuildOnlyAttribute"/>; in DEBUG
    /// only the ones marked with <see cref="TestCommandAttribute"/>, and all of them scoped to the logs guild.
    /// </summary>
    public static void AddDiscoveredSlashCommands(this CommandsExtension extension, ulong logGuildId)
    {
        Register(extension, SlashCommandsNamespace, logGuildId);
    }

    public static void AddDiscoveredContextMenuCommands(this CommandsExtension extension, ulong logGuildId)
    {
        Register(extension, ContextMenuCommandsNamespace, logGuildId);
    }

    private static void Register(CommandsExtension extension, string ns, ulong logGuildId)
    {
        Type[] types = [.. DiscoverCommandTypes(ns)];

        if (types.Length == 0)
        {
            return;
        }

#if DEBUG
        extension.AddCommands(types, logGuildId);
#else
        Type[] logGuildOnly = [.. types.Where(type => type.GetCustomAttribute<LogGuildOnlyAttribute>() is not null)];
        Type[] global = [.. types.Except(logGuildOnly)];

        if (global.Length > 0)
        {
            extension.AddCommands(global);
        }

        if (logGuildOnly.Length > 0)
        {
            extension.AddCommands(logGuildOnly, logGuildId);
        }
#endif
    }

    private static IEnumerable<Type> DiscoverCommandTypes(string ns)
    {
        IEnumerable<Type> commandTypes = typeof(TestCommandAttribute).Assembly
            .GetTypes()
            .Where(type => type.Namespace == ns
                           && type is { IsClass: true, IsAbstract: false }
                           && (type.GetCustomAttribute<CommandAttribute>() is not null
                               || type.GetMethods().Any(method => method.GetCustomAttribute<CommandAttribute>() is not null)));

#if DEBUG
        commandTypes = commandTypes.Where(type => type.GetCustomAttribute<TestCommandAttribute>() is not null);
#endif

        return commandTypes;
    }
}
