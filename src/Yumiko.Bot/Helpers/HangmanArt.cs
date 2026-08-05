namespace Yumiko.Bot.Helpers;

/// <summary>
/// The hangman figure, drawn with box characters. The dots act as spaces because
/// Discord collapses consecutive spaces outside a code block.
/// </summary>
public static class HangmanArt
{
    private const string Gallows =
        ". ┌─────┐\n" +
        ".┃...............┋\n" +
        ".┃...............┋\n";

    private static readonly string[] Bodies =
    [
        ".┃\n.┃\n.┃\n",
        ".┃.............:dizzy_face: \n.┃\n.┃\n",
        ".┃.............:dizzy_face: \n.┃............./\n.┃\n",
        ".┃.............:dizzy_face: \n.┃............./ |\n.┃\n",
        ".┃.............:dizzy_face: \n.┃............./ | \\   \n.┃\n",
        ".┃.............:dizzy_face: \n.┃............./ | \\   \n.┃............../\n",
        ".┃.............:dizzy_face: \n.┃............./ | \\   \n.┃............../\\ \n",
    ];

    /// <summary>Amount of misses that end the match.</summary>
    public static int MaxMistakes => Bodies.Length - 1;

    public static string Draw(int mistakes) =>
        mistakes < 0 || mistakes >= Bodies.Length
            ? string.Empty
            : $"{Gallows}{Bodies[mistakes]}/-\\\n";
}
