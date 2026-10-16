namespace Yumiko.Bot.Configuration;

/// <summary>Execution environment: on debug the commands are registered only on the logs guild.</summary>
public static class BotEnvironment
{
#if DEBUG
    public static readonly bool IsDebug = true;
#else
    public static readonly bool IsDebug = false;
#endif
}
