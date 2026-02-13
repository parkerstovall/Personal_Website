using ChessApi.HelperClasses.Chess;
using ChessApi.Models.DB;
using ChessApi.Pieces;
using ChessApi.Pieces.Interfaces;

namespace ChessApi.Tests;

public class MovePipelineTests
{
    [Fact]
    public void MovePiece_ResetsAndRecalculatesPressure()
    {
        var game = TestHelpers.EmptyGame();
        var board = game.Board;

        // White rook at a1 (7,0) puts pressure on column 0
        TestHelpers.PlacePiece(board, 7, 0, new Rook(false));
        TestHelpers.PlacePiece(board, 0, 4, new King(true));

        // Move the rook to a4 (7,3)
        MoveHelper.MovePiece([7, 0], [7, 3], ref game);

        // After move, pressure should be recalculated
        // The rook is now at (7,3) and should put pressure on row 7 and column 3
        Assert.True(board.Rows[0].Squares[3].WhitePressure > 0);
    }

    [Fact]
    public void DoublePawnPush_SetsEnPassantFlag()
    {
        var game = TestHelpers.EmptyGame();
        var board = game.Board;

        // White pawn at starting position
        var pawn = new Pawn(false); // white
        TestHelpers.PlacePiece(board, 6, 4, pawn);
        TestHelpers.PlacePiece(board, 0, 4, new King(true));
        TestHelpers.PlacePiece(board, 7, 4, new King(false));

        // Double push from row 6 to row 4
        MoveHelper.MovePiece([6, 4], [4, 4], ref game);

        // En passant square should be set at row 5 col 4
        Assert.Equal(false, board.Rows[5].Squares[4].EnPassantColor);
    }

    [Fact]
    public void PawnPromotion_ReplacesWithQueen()
    {
        var game = TestHelpers.EmptyGame();
        var board = game.Board;

        // Black pawn about to promote
        var pawn = new Pawn(true) { HasMoved = true };
        TestHelpers.PlacePiece(board, 6, 4, pawn);
        TestHelpers.PlacePiece(board, 0, 0, new King(false));
        TestHelpers.PlacePiece(board, 0, 7, new King(true));

        // Move to last rank
        MoveHelper.MovePiece([6, 4], [7, 4], ref game);

        var promotedPiece = TestHelpers.GetPiece(board, 7, 4);
        Assert.IsType<Queen>(promotedPiece);
        Assert.True(promotedPiece!.Color); // should keep black color
    }

    [Fact]
    public void WhitePawnPromotion_ReplacesWithQueen()
    {
        var game = TestHelpers.EmptyGame();
        var board = game.Board;

        var pawn = new Pawn(false) { HasMoved = true }; // white pawn
        TestHelpers.PlacePiece(board, 1, 4, pawn);
        TestHelpers.PlacePiece(board, 7, 0, new King(false));
        TestHelpers.PlacePiece(board, 7, 7, new King(true));

        MoveHelper.MovePiece([1, 4], [0, 4], ref game);

        var promotedPiece = TestHelpers.GetPiece(board, 0, 4);
        Assert.IsType<Queen>(promotedPiece);
        Assert.False(promotedPiece!.Color); // should keep white color
    }

    [Fact]
    public void NormalCapture_RemovesEnemyPiece()
    {
        var game = TestHelpers.EmptyGame();
        var board = game.Board;

        TestHelpers.PlacePiece(board, 4, 4, new Rook(false));
        TestHelpers.PlacePiece(board, 4, 7, new Knight(true));
        TestHelpers.PlacePiece(board, 0, 0, new King(true));
        TestHelpers.PlacePiece(board, 7, 7, new King(false));

        MoveHelper.MovePiece([4, 4], [4, 7], ref game);

        // Rook should be at destination
        Assert.IsType<Rook>(TestHelpers.GetPiece(board, 4, 7));
        // Source should be empty
        Assert.Null(TestHelpers.GetPiece(board, 4, 4));
    }

    [Fact]
    public void MovePiece_SetsPieceHasMoved()
    {
        var game = TestHelpers.EmptyGame();
        var board = game.Board;

        var rook = new Rook(false);
        TestHelpers.PlacePiece(board, 7, 0, rook);
        TestHelpers.PlacePiece(board, 0, 0, new King(true));
        TestHelpers.PlacePiece(board, 7, 4, new King(false));

        Assert.False(rook.HasMoved);

        MoveHelper.MovePiece([7, 0], [5, 0], ref game);

        // The piece at the new location should have HasMoved set
        var movedRook = TestHelpers.GetPiece(board, 5, 0) as Rook;
        Assert.NotNull(movedRook);
        Assert.True(movedRook.HasMoved);
    }

    [Fact]
    public void MovePiece_ClearsPinsFromPreviousMove()
    {
        var game = TestHelpers.EmptyGame();
        var board = game.Board;

        // Set up a pinned piece and verify pin gets cleared on next move
        var knight = new Knight(true) { PinnedDir = Direction.FromTopToBottom };
        TestHelpers.PlacePiece(board, 4, 4, knight);
        TestHelpers.PlacePiece(board, 0, 0, new King(true));
        TestHelpers.PlacePiece(board, 7, 7, new King(false));

        // Move something to trigger board refresh (move a different piece)
        var pawn = new Pawn(true) { HasMoved = true };
        TestHelpers.PlacePiece(board, 3, 0, pawn);
        MoveHelper.MovePiece([3, 0], [4, 0], ref game);

        // After board refresh, pins should be recalculated
        // The knight's pin should be cleared since there's no pinning piece
        Assert.Equal(Direction.None, knight.PinnedDir);
    }
}
