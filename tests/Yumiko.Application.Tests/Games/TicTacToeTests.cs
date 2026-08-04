using Yumiko.Application.Games;
using Yumiko.Model.Enum;

namespace Yumiko.Application.Tests.Games;

public class TicTacToeTests
{
    // "x" = Player1, "o" = Player2, "." = vacío.
    private static List<TicTacToeCell> Board(string cells) =>
        [.. cells.Where(c => c is 'x' or 'o' or '.').Select(c => c switch
        {
            'x' => TicTacToeCell.Player1,
            'o' => TicTacToeCell.Player2,
            _ => TicTacToeCell.Empty,
        })];

    [Theory]
    [InlineData("xxx......")] // horizontal 1
    [InlineData("...xxx...")] // horizontal 2
    [InlineData("......xxx")] // horizontal 3
    [InlineData("x..x..x..")] // vertical 1
    [InlineData(".x..x..x.")] // vertical 2
    [InlineData("..x..x..x")] // vertical 3
    [InlineData("x...x...x")] // diagonal principal
    [InlineData("..x.x.x..")] // diagonal inversa
    public void DetectsTheEightWinningLines(string cells)
    {
        var board = Board(cells);

        Assert.True(TicTacToe.HasWinningLine(board, TicTacToeCell.Player1));
        Assert.False(TicTacToe.HasWinningLine(board, TicTacToeCell.Player2));

        var (finished, winner) = TicTacToe.Result(board);
        Assert.True(finished);
        Assert.Equal(TicTacToeCell.Player1, winner);
    }

    [Fact]
    public void AMixedLineDoesNotWin()
    {
        var board = Board("xox......");

        Assert.False(TicTacToe.HasWinningLine(board, TicTacToeCell.Player1));
        Assert.False(TicTacToe.HasWinningLine(board, TicTacToeCell.Player2));
    }

    [Fact]
    public void FullBoardWithoutALine_IsATie()
    {
        // x o x
        // x o o
        // o x x
        var board = Board("xoxxoooxx");

        var (finished, winner) = TicTacToe.Result(board);

        Assert.True(finished);
        Assert.Equal(TicTacToeCell.Empty, winner);
        Assert.False(TicTacToe.HasMovesLeft(board));
    }

    [Fact]
    public void AGameInProgressIsNotFinished()
    {
        var board = Board("xo.......");

        var (finished, winner) = TicTacToe.Result(board);

        Assert.False(finished);
        Assert.Equal(TicTacToeCell.Empty, winner);
        Assert.True(TicTacToe.HasMovesLeft(board));
    }

    [Fact]
    public void EmptyBoard_NobodyWins()
    {
        var board = Board(".........");

        Assert.False(TicTacToe.HasWinningLine(board, TicTacToeCell.Player1));
        Assert.False(TicTacToe.HasWinningLine(board, TicTacToeCell.Player2));
        // Un tablero vacío no puede hacer ganar al "jugador vacío".
        Assert.False(TicTacToe.HasWinningLine(board, TicTacToeCell.Empty));
    }

    [Fact]
    public void TheSecondPlayerWins()
    {
        var board = Board("ooo.x.x..");

        var (finished, winner) = TicTacToe.Result(board);

        Assert.True(finished);
        Assert.Equal(TicTacToeCell.Player2, winner);
    }

    [Fact]
    public void BoardOfInvalidSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TicTacToe.HasWinningLine(Board("xxx"), TicTacToeCell.Player1));
    }
}
