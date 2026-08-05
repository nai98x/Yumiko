using DSharpPlus.Entities;
using Yumiko.Model.Enum;

namespace Yumiko.Bot.Helpers;

/// <summary>
/// The buttons <em>are</em> the board: the style and being disabled tell whose each cell is.
/// This helper translates between that representation and the <c>TicTacToeCell[]</c> understood by
/// <c>Application.Games.TicTacToe</c>.
/// </summary>
public static class TicTacToeBoard
{
    private static readonly string[] Ids = ["a", "b", "c", "d", "e", "f", "g", "h", "i"];

    public static List<DiscordButtonComponent> Initial() =>
        [.. Ids.Select(id => new DiscordButtonComponent(DiscordButtonStyle.Secondary, id, "[ ]"))];

    /// <summary>Marks the clicked cell with the token of the player on turn and disables it.</summary>
    public static List<DiscordButtonComponent> Mark(IReadOnlyList<DiscordButtonComponent> buttons, string clickedId, bool isFirstPlayer) =>
    [
        .. buttons.Select(button => button.CustomId == clickedId
            ? new DiscordButtonComponent(
                isFirstPlayer ? DiscordButtonStyle.Success : DiscordButtonStyle.Danger,
                clickedId,
                isFirstPlayer ? "[X]" : "[O]",
                true)
            : button),
    ];

    public static List<DiscordButtonComponent> DisableAll(IReadOnlyList<DiscordButtonComponent> buttons) =>
        [.. buttons.Select(button => button.Disable())];

    public static List<TicTacToeCell> Read(IReadOnlyList<DiscordButtonComponent> buttons) =>
    [
        .. buttons.Select(button => !button.Disabled
            ? TicTacToeCell.Empty
            : button.Style switch
            {
                DiscordButtonStyle.Success => TicTacToeCell.Player1,
                DiscordButtonStyle.Danger => TicTacToeCell.Player2,
                _ => TicTacToeCell.Empty,
            }),
    ];
}
