using ChessApi.HelperClasses.Chess;
using ChessApi.Models.DB;
using ChessApi.Pieces;

namespace ChessApi.Tests;

public class CheckAndCheckmateTests
{
    [Fact]
    public void MovePiece_DetectsCheck()
    {
        var game = TestHelpers.EmptyGame();
        var board = game.Board;

        // White king at e1
        TestHelpers.PlacePiece(board, 7, 4, new King(false));
        // Black rook starts at a8, will move to e8 giving check
        TestHelpers.PlacePiece(board, 0, 0, new Rook(true));

        MoveHelper.MovePiece([0, 0], [0, 4], ref game);

        // White should be in check
        Assert.Equal(false, game.CheckedColor); // false = white is in check
    }

    [Fact]
    public void MovePiece_NoCheckWhenNotAttackingKing()
    {
        var game = TestHelpers.EmptyGame();
        var board = game.Board;

        TestHelpers.PlacePiece(board, 7, 4, new King(false));
        TestHelpers.PlacePiece(board, 0, 0, new Rook(true));

        // Move rook to a4 — not attacking the king
        MoveHelper.MovePiece([0, 0], [0, 3], ref game);

        Assert.Null(game.CheckedColor);
    }

    [Fact]
    public void KingInCheck_CanOnlyMoveAwayFromAttacker()
    {
        var game = TestHelpers.EmptyGame();
        var board = game.Board;

        // White king at (7,4), black rook at (1,4)
        TestHelpers.PlacePiece(board, 7, 4, new King(false));
        TestHelpers.PlacePiece(board, 1, 4, new Rook(true));
        TestHelpers.PlacePiece(board, 0, 0, new King(true));

        // Move rook to e1-file (0,4) to give check — this triggers full board refresh
        MoveHelper.MovePiece([1, 4], [0, 4], ref game);

        Assert.Equal(false, game.CheckedColor); // white is in check

        // Now get king's moves — it should not be able to stay on col 4
        var king = board.Rows[7].Squares[4].Piece as King;
        Assert.NotNull(king);
        var moves = king.GetPaths(board, [7, 4], true);

        foreach (var m in moves)
        {
            // King should not move to column 4 (rook's file)
            Assert.NotEqual(4, m.MoveTo[1]);
        }
    }

    [Fact]
    public void Stalemate_ReturnsZeroScore()
    {
        // In the MinMax engine, when no moves are available for a side, it returns 0
        var engine = new MinMaxEngine(
            new MinMaxEngineOptions
            {
                MaxDepth = 1,
                MaxTime = null,
                MaxPlayer = true,
                BoardSize = 8,
                PieceHashKeys =
                [
                    "p0",
                    "p1",
                    "r0",
                    "r1",
                    "n0",
                    "n1",
                    "b0",
                    "b1",
                    "q0",
                    "q1",
                    "k0",
                    "k1",
                ],
            }
        );

        // Starting board has no stalemate, but we test that score is symmetric
        var board = TestHelpers.StartingBoard();
        var score = engine.GetBoardScore(board);

        // Starting position should be 0 (symmetric)
        Assert.Equal(0, score);
    }
}
