using Yumiko.Model.Enum;

namespace Yumiko.Application.Games;

public static class TicTacToe
{
    /// <summary>Cantidad de casillas del tablero.</summary>
    public const int Cells = 9;

    private static readonly int[][] Lines =
    [
        [0, 1, 2], [3, 4, 5], [6, 7, 8], // horizontales
        [0, 3, 6], [1, 4, 7], [2, 5, 8], // verticales
        [0, 4, 8], [2, 4, 6],            // diagonales
    ];

    public static bool HasWinningLine(IReadOnlyList<TicTacToeCell> board, TicTacToeCell player)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(board.Count, Cells);

        if (player == TicTacToeCell.Empty)
        {
            return false;
        }

        return Lines.Any(line => line.All(i => board[i] == player));
    }

    public static bool HasMovesLeft(IReadOnlyList<TicTacToeCell> board) =>
        board.Any(c => c == TicTacToeCell.Empty);

    /// <summary>
    /// Estado de la partida: si terminó y, en ese caso, quién ganó
    /// (<see cref="TicTacToeCell.Empty"/> significa empate).
    /// </summary>
    public static (bool Finished, TicTacToeCell Winner) Result(IReadOnlyList<TicTacToeCell> board)
    {
        if (HasWinningLine(board, TicTacToeCell.Player1))
        {
            return (true, TicTacToeCell.Player1);
        }

        if (HasWinningLine(board, TicTacToeCell.Player2))
        {
            return (true, TicTacToeCell.Player2);
        }

        return HasMovesLeft(board) ? (false, TicTacToeCell.Empty) : (true, TicTacToeCell.Empty);
    }
}
