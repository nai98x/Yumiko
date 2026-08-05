namespace Yumiko.Bot.Commands.Framework.Attributes;

/// <summary>
/// Marks a command class so it is also registered on DEBUG builds. Without this, on debug
/// the class is not registered and the test bot does not get filled with half ported commands.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class TestCommandAttribute : Attribute;
