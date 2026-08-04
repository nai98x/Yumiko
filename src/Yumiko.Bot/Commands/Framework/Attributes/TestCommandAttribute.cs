namespace Yumiko.Bot.Commands.Framework.Attributes;

/// <summary>
/// Marca una clase de comandos para que se registre también en builds de DEBUG. Sin esto, en debug
/// la clase no se registra y así el bot de test no llena el guild de comandos a medio portar.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class TestCommandAttribute : Attribute;
