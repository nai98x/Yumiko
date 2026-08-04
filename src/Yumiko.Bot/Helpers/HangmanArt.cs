namespace Yumiko.Bot.Helpers;

/// <summary>
/// El muñeco del ahorcado, dibujado con caracteres de caja. Los puntos hacen de espacios porque
/// Discord colapsa los espacios consecutivos fuera de un bloque de código.
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

    /// <summary>Cantidad de errores que terminan la partida.</summary>
    public static int MaxMistakes => Bodies.Length - 1;

    public static string Draw(int mistakes) =>
        mistakes < 0 || mistakes >= Bodies.Length
            ? string.Empty
            : $"{Gallows}{Bodies[mistakes]}/-\\\n";
}
