using ChessApi.Models.API;
using ChessApi.Pieces;

namespace ChessApi.Tests;

public class HasMovedTests
{
    [Theory]
    [InlineData(1, false)] // Pawn starting row (black)
    [InlineData(6, false)] // Pawn starting row (white)
    [InlineData(3, true)] // Pawn has moved
    [InlineData(4, true)] // Pawn has moved
    [InlineData(0, true)] // Pawn on back rank (promoted) - not a starting pawn row
    [InlineData(7, true)] // Pawn on back rank (promoted) - not a starting pawn row
    public void Pawn_HasMoved_ChecksMoveFromRow(int fromRow, bool expectedHasMoved)
    {
        var move = new PossibleMove
        {
            MoveFrom = [fromRow, 3],
            MoveTo = [fromRow + 1, 3],
            MovingPiece = new Pawn(true),
        };

        Assert.Equal(expectedHasMoved, move.HasMoved);
    }

    [Theory]
    [InlineData(0, false)] // Back rank (starting position)
    [InlineData(7, false)] // Back rank (starting position)
    [InlineData(1, true)] // Not a starting row for non-pawn
    [InlineData(3, true)] // Middle of board
    [InlineData(6, true)] // Not a starting row for non-pawn
    public void NonPawn_HasMoved_ChecksMoveFromRow(int fromRow, bool expectedHasMoved)
    {
        var move = new PossibleMove
        {
            MoveFrom = [fromRow, 4],
            MoveTo = [fromRow + 1, 4],
            MovingPiece = new Knight(true),
        };

        Assert.Equal(expectedHasMoved, move.HasMoved);
    }

    [Fact]
    public void HasMoved_UsesMoveFrom_NotMoveTo()
    {
        // MoveFrom on starting row, MoveTo on non-starting row
        var move = new PossibleMove
        {
            MoveFrom = [0, 4],
            MoveTo = [3, 4], // destination is irrelevant
            MovingPiece = new Knight(true),
        };

        Assert.False(move.HasMoved);

        // MoveFrom on non-starting row, MoveTo on starting row
        var move2 = new PossibleMove
        {
            MoveFrom = [3, 4],
            MoveTo = [0, 4], // destination is irrelevant
            MovingPiece = new Knight(true),
        };

        Assert.True(move2.HasMoved);
    }
}
