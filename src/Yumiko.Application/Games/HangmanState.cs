using System.Text;
using System.Text.RegularExpressions;

namespace Yumiko.Application.Games;

/// <summary>
/// State of a hangman match: which letters were guessed, how many misses there are and how the masked
/// word looks. It knows nothing about Discord or scores.
/// </summary>
public sealed partial class HangmanState
{
    /// <summary>Misses that lose the match.</summary>
    public const int MaxMistakes = 6;

    private readonly char[] _word;
    private readonly HashSet<char> _guessed = [];
    private readonly List<string> _usedLetters = [];

    public HangmanState(string word)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(word);

        // Repeated spaces are collapsed into a single one.
        _word = [.. RepeatedWhitespace().Replace(word.ToLowerInvariant().Trim(), " ")];

        // Everything that is not a letter (spaces, colons, dashes) is shown from the start.
        foreach (char c in _word.Where(c => !char.IsLetter(c)))
        {
            _guessed.Add(c);
        }
    }

    public int Mistakes { get; private set; }

    public IReadOnlyList<string> UsedLetters => _usedLetters;

    /// <summary>The normalized word, character by character, so the presentation can format it.</summary>
    public IReadOnlyList<char> Word => _word;

    public bool IsRevealed(char character) => _guessed.Contains(character);

    public bool IsLost => Mistakes >= MaxMistakes;

    public bool IsComplete => _word.Where(char.IsLetter).All(_guessed.Contains);

    public bool IsFinished => IsLost || IsComplete;

    /// <summary>
    /// Tries a letter. Returns <c>true</c> if it was in the word; otherwise it adds a miss.
    /// The letter is recorded in <see cref="UsedLetters"/> either way.
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
    /// Reveals the whole word (someone guessed it in one go). Returns how many letters were left.
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

    /// <summary>Adds a miss without consuming a letter attempt (timeouts, cancellations).</summary>
    public void AddMistake() => Mistakes++;

    /// <summary>Forces the loss (match cancellation).</summary>
    public void Surrender() => Mistakes = MaxMistakes;

    /// <summary>The word with the guessed letters visible and the rest as <c>_</c>.</summary>
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
