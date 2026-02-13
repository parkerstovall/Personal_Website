using ChessApi.HelperClasses.Chess;
using ChessApi.Models.API;
using ChessApi.Models.DB;
using ChessApi.Pieces;
using ChessApi.Pieces.Interfaces;

namespace ChessApi.Tests;

public class CastleTests
{
    [Fact]
    public void KingsideCastle_MovesRookCorrectly()
    {
        var game = TestHelpers.EmptyGame();
        var board = game.Board;

        // Place white king at e1 (row 7, col 4) and rook at h1 (row 7, col 7)
        var king = new King(false);
        var rook = new Rook(false);
        TestHelpers.PlacePiece(board, 7, 4, king);
        TestHelpers.PlacePiece(board, 7, 7, rook);

        // Move king from e1 to g1 (kingside castle)
        MoveHelper.MovePiece([7, 4], [7, 6], ref game);

        // King should be at g1
        Assert.IsType<King>(TestHelpers.GetPiece(board, 7, 6));
        // Rook should have moved from h1 to f1
        Assert.IsType<Rook>(TestHelpers.GetPiece(board, 7, 5));
        // Original rook square should be empty
        Assert.Null(TestHelpers.GetPiece(board, 7, 7));
        // Original king square should be empty
        Assert.Null(TestHelpers.GetPiece(board, 7, 4));
    }

    [Fact]
    public void QueensideCastle_MovesRookCorrectly()
    {
        var game = TestHelpers.EmptyGame();
        var board = game.Board;

        // Place white king at e1 (row 7, col 4) and rook at a1 (row 7, col 0)
        var king = new King(false);
        var rook = new Rook(false);
        TestHelpers.PlacePiece(board, 7, 4, king);
        TestHelpers.PlacePiece(board, 7, 0, rook);

        // Move king from e1 to c1 (queenside castle)
        MoveHelper.MovePiece([7, 4], [7, 2], ref game);

        // King should be at c1
        Assert.IsType<King>(TestHelpers.GetPiece(board, 7, 2));
        // Rook should have moved from a1 to d1
        Assert.IsType<Rook>(TestHelpers.GetPiece(board, 7, 3));
        // Original rook square should be empty
        Assert.Null(TestHelpers.GetPiece(board, 7, 0));
    }

    [Fact]
    public void Castle_SetsHasMovedOnRook()
    {
        var game = TestHelpers.EmptyGame();
        var board = game.Board;

        var king = new King(false);
        var rook = new Rook(false);
        TestHelpers.PlacePiece(board, 7, 4, king);
        TestHelpers.PlacePiece(board, 7, 7, rook);

        Assert.False(rook.HasMoved);

        MoveHelper.MovePiece([7, 4], [7, 6], ref game);

        // The rook at its new position should have HasMoved = true
        var movedRook = TestHelpers.GetPiece(board, 7, 5) as Rook;
        Assert.NotNull(movedRook);
        Assert.True(movedRook.HasMoved);
    }

    [Fact]
    public void Castle_SetsHasMovedOnKing()
    {
        var game = TestHelpers.EmptyGame();
        var board = game.Board;

        var king = new King(false);
        var rook = new Rook(false);
        TestHelpers.PlacePiece(board, 7, 4, king);
        TestHelpers.PlacePiece(board, 7, 7, rook);

        Assert.False(king.HasMoved);

        MoveHelper.MovePiece([7, 4], [7, 6], ref game);

        var movedKing = TestHelpers.GetPiece(board, 7, 6) as King;
        Assert.NotNull(movedKing);
        Assert.True(movedKing.HasMoved);
    }
}
