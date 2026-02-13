using ChessApi.HelperClasses.Chess;
using ChessApi.Models.API;
using ChessApi.Pieces;

namespace ChessApi.Tests;

public class CopyGameTests
{
    private readonly MinMaxEngine _engine;

    public CopyGameTests()
    {
        _engine = new MinMaxEngine(
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
    }

    [Fact]
    public void CopyGame_PreservesAllPieces()
    {
        var game = TestHelpers.NewGame();
        var copy = _engine.CopyGame(game);

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                var origPiece = game.Board.Rows[i].Squares[j].Piece;
                var copyPiece = copy.Board.Rows[i].Squares[j].Piece;

                if (origPiece is null)
                {
                    Assert.Null(copyPiece);
                }
                else
                {
                    Assert.NotNull(copyPiece);
                    Assert.Equal(origPiece.GetType(), copyPiece.GetType());
                    Assert.Equal(origPiece.Color, copyPiece.Color);
                    Assert.Equal(origPiece.Value, copyPiece.Value);
                }
            }
        }
    }

    [Fact]
    public void CopyGame_IsIndependent_ModifyingCopyDoesNotAffectOriginal()
    {
        var game = TestHelpers.NewGame();
        var copy = _engine.CopyGame(game);

        // Remove a piece from the copy
        copy.Board.Rows[0].Squares[0].Piece = null;

        // Original should still have the piece
        Assert.NotNull(game.Board.Rows[0].Squares[0].Piece);
    }

    [Fact]
    public void CopyGame_PiecesAreDeepCopied()
    {
        var game = TestHelpers.NewGame();
        var copy = _engine.CopyGame(game);

        // Pieces should be different instances
        var origPiece = game.Board.Rows[0].Squares[0].Piece;
        var copyPiece = copy.Board.Rows[0].Squares[0].Piece;

        Assert.NotNull(origPiece);
        Assert.NotNull(copyPiece);
        Assert.NotSame(origPiece, copyPiece);
    }

    [Fact]
    public void CopyGame_PreservesCheckedColor()
    {
        var game = TestHelpers.NewGame();
        game.CheckedColor = false; // white in check

        var copy = _engine.CopyGame(game);

        Assert.Equal(false, copy.CheckedColor);
    }

    [Fact]
    public void CopyGame_CoordsAreShared()
    {
        // After our optimization, Coords references should be shared
        var game = TestHelpers.NewGame();
        var copy = _engine.CopyGame(game);

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Assert.Same(
                    game.Board.Rows[i].Squares[j].Coords,
                    copy.Board.Rows[i].Squares[j].Coords
                );
            }
        }
    }
}
