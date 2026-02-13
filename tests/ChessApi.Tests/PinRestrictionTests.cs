using ChessApi.HelperClasses.Chess;
using ChessApi.Pieces;

namespace ChessApi.Tests;

public class PinRestrictionTests
{
    [Fact]
    public void PinnedKnight_CannotMove()
    {
        var knight = new Knight(true) { PinnedDir = Direction.FromTopToBottom };
        var board = TestHelpers.EmptyBoard();
        TestHelpers.PlacePiece(board, 4, 4, knight);

        var moves = knight.GetPaths(board, [4, 4], false);

        Assert.Empty(moves);
    }

    [Fact]
    public void PinnedRookOnDiagonal_CannotMove()
    {
        // Rook can only move straight; pinned on a diagonal means no valid moves
        var result = PieceHelper.GetIncrements(
            Direction.FromTopLeftToBottomRight,
            diag: false,
            straight: true
        );

        Assert.Empty(result.Item1);
    }

    [Fact]
    public void PinnedBishopOnStraightLine_CannotMove()
    {
        var result = PieceHelper.GetIncrements(
            Direction.FromTopToBottom,
            diag: true,
            straight: false
        );

        Assert.Empty(result.Item1);
    }

    [Fact]
    public void PinnedRookOnStraightLine_CanMoveAlongPin()
    {
        var result = PieceHelper.GetIncrements(
            Direction.FromTopToBottom,
            diag: false,
            straight: true
        );

        // Should have 2 directions (up and down along the pin)
        Assert.Equal(2, result.Item1.Length);
    }

    [Fact]
    public void PinnedBishopOnDiagonal_CanMoveAlongPin()
    {
        var result = PieceHelper.GetIncrements(
            Direction.FromTopLeftToBottomRight,
            diag: true,
            straight: false
        );

        // Should have 2 directions along the diagonal
        Assert.Equal(2, result.Item1.Length);
    }

    [Fact]
    public void PinnedQueen_CanMoveAlongStraightPin()
    {
        var result = PieceHelper.GetIncrements(
            Direction.FromLeftToRight,
            diag: true,
            straight: true
        );

        // Queen pinned left-right can only move left and right
        Assert.Equal(2, result.Item1.Length);
    }

    [Fact]
    public void PinnedQueen_CanMoveAlongDiagonalPin()
    {
        var result = PieceHelper.GetIncrements(
            Direction.FromTopLeftToBottomRight,
            diag: true,
            straight: true
        );

        // Queen pinned on diagonal can only move along that diagonal
        Assert.Equal(2, result.Item1.Length);
    }

    [Fact]
    public void GetSingleIncrement_ReturnsSameInstances()
    {
        var inc1 = PieceHelper.GetSingleIncrement(Direction.FromTopToBottom);
        var inc2 = PieceHelper.GetSingleIncrement(Direction.FromTopToBottom);

        Assert.Same(inc1, inc2);
    }

    [Fact]
    public void GetSingleIncrement_AllDirectionsReturnCorrectValues()
    {
        Assert.Equal(
            new[] { 1, 1 },
            PieceHelper.GetSingleIncrement(Direction.FromTopLeftToBottomRight)
        );
        Assert.Equal(
            new[] { 1, -1 },
            PieceHelper.GetSingleIncrement(Direction.FromTopRightToBottomLeft)
        );
        Assert.Equal(
            new[] { -1, -1 },
            PieceHelper.GetSingleIncrement(Direction.FromBottomRightToTopLeft)
        );
        Assert.Equal(
            new[] { -1, 1 },
            PieceHelper.GetSingleIncrement(Direction.FromBottomLeftToTopRight)
        );
        Assert.Equal(new[] { 0, 1 }, PieceHelper.GetSingleIncrement(Direction.FromLeftToRight));
        Assert.Equal(new[] { 0, -1 }, PieceHelper.GetSingleIncrement(Direction.FromRightToLeft));
        Assert.Equal(new[] { 1, 0 }, PieceHelper.GetSingleIncrement(Direction.FromTopToBottom));
        Assert.Equal(new[] { -1, 0 }, PieceHelper.GetSingleIncrement(Direction.FromBottomToTop));
    }
}
