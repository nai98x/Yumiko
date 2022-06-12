namespace Yumiko.Enums
{
    using DSharpPlus.SlashCommands;

    public enum Difficulty
    {
        [ChoiceName("Easy")]
        Easy,
        [ChoiceName("Normal")]
        Normal,
        [ChoiceName("Hard")]
        Hard,
        [ChoiceName("Extreme")]
        Extreme
    }
}
