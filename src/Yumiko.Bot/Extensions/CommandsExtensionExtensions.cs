using System.Reflection;
using DSharpPlus.Commands;
using Yumiko.Bot.Commands.Framework.Attributes;

namespace Yumiko.Bot.Extensions;

public static class CommandsExtensionExtensions
{
    private const string SlashCommandsNamespace = "Yumiko.Bot.Commands.Slash";
    private const string ContextMenuCommandsNamespace = "Yumiko.Bot.Commands.ContextMenu";

    /// <summary>
    /// Descubre por reflexión las clases de slash command del namespace correspondiente. En RELEASE
    /// se registran globalmente; en DEBUG solo las marcadas con <see cref="TestCommandAttribute"/>,
    /// y acotadas al guild de logs.
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
        extension.AddCommands(types);
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
