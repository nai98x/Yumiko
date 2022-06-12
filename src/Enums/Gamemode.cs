namespace Yumiko.Enums
{
    using DSharpPlus.SlashCommands;

    public enum Gamemode
    {
        [ChoiceName("Characters")]
        Characters,
        [ChoiceName("Animes")]
        Animes,
        [ChoiceName("Mangas")]
        Mangas,
        [ChoiceName("Protagonists")]
        Protagonists,
        [ChoiceName("Genres")]
        Genres,
        [ChoiceName("Studios")]
        Studios
    }
}
