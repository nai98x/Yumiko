using System.Text;
using System.Text.RegularExpressions;

namespace Yumiko.Application.Games;

/// <summary>
/// Estado de una partida de ahorcado: qué letras se adivinaron, cuántos errores van y cómo se ve la
/// palabra enmascarada. No sabe nada de Discord ni de puntajes.
/// </summary>
public sealed partial class HangmanState
{
    /// <summary>Errores que dan la partida por perdida.</summary>
    public const int MaxMistakes = 6;

    private readonly char[] _word;
    private readonly HashSet<char> _guessed = [];
    private readonly List<string> _usedLetters = [];

    public HangmanState(string word)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(word);

        // Los espacios repetidos se colapsan en uno solo.
        _word = [.. RepeatedWhitespace().Replace(word.ToLowerInvariant().Trim(), " ")];

        // Todo lo que no es letra (espacios, dos puntos, guiones) se muestra desde el arranque.
        foreach (char c in _word.Where(c => !char.IsLetter(c)))
        {
            _guessed.Add(c);
        }
    }

    public int Mistakes { get; private set; }

    public IReadOnlyList<string> UsedLetters => _usedLetters;

    /// <summary>La palabra normalizada, carácter por carácter, para que la presentación la formatee.</summary>
    public IReadOnlyList<char> Word => _word;

    public bool IsRevealed(char character) => _guessed.Contains(character);

    public bool IsLost => Mistakes >= MaxMistakes;

    public bool IsComplete => _word.Where(char.IsLetter).All(_guessed.Contains);

    public bool IsFinished => IsLost || IsComplete;

    /// <summary>
    /// Prueba una letra. Devuelve <c>true</c> si estaba en la palabra; si no, suma un error.
    /// La letra queda registrada en <see cref="UsedLetters"/> en cualquier caso.
    /// </summary>
    public bool Guess(string letter)
    {
        string normalized = (letter ?? string.Empty).ToLowerInvariant().Trim();

        if (!_usedLetters.Contains(normalized))
        {
            _usedLetters.Add(normalized);
        }

        bool isCorrect = normalized.Length == 1 && _word.Contains(normalized[0]) && char.IsLetter(normalized[0]);

        if (isCorrect)
        {
            _guessed.Add(normalized[0]);
        }
        else
        {
            Mistakes++;
        }

        return isCorrect;
    }

    /// <summary>
    /// Revela la palabra entera (alguien la adivinó de una). Devuelve cuántas letras faltaban.
    /// </summary>
    public int RevealAll()
    {
        int remaining = _word.Where(char.IsLetter).Distinct().Count(c => !_guessed.Contains(c));

        foreach (char c in _word)
        {
            _guessed.Add(c);
        }

        return remaining;
    }

    /// <summary>Suma un error sin consumir un intento de letra (timeouts, cancelaciones).</summary>
    public void AddMistake() => Mistakes++;

    /// <summary>Fuerza la derrota (cancelación de la partida).</summary>
    public void Surrender() => Mistakes = MaxMistakes;

    /// <summary>La palabra con las letras adivinadas visibles y el resto como <c>_</c>.</summary>
    public string Masked()
    {
        StringBuilder sb = new();

        foreach (char c in _word)
        {
            sb.Append(_guessed.Contains(c) ? c : '_').Append(' ');
        }

        return sb.ToString().TrimEnd();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex RepeatedWhitespace();
}
