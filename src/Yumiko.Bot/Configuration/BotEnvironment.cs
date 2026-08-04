namespace Yumiko.Bot.Configuration;

/// <summary>Entorno de ejecución: en debug los comandos se registran solo en el guild de logs.</summary>
public static class BotEnvironment
{
#if DEBUG
    public const bool IsDebug = true;
#else
    public const bool IsDebug = false;
#endif
}
