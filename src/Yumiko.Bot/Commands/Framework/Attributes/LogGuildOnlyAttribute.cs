namespace Yumiko.Bot.Commands.Framework.Attributes;

/// <summary>
/// Registers the class scoped to the logs guild even on release, so its commands do not show up
/// on the rest of the servers.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LogGuildOnlyAttribute : Attribute;
