using ChessApi.HelperClasses.Chess;
using ChessApi.Models.API;
using ChessApi.Pieces;

namespace ChessApi.Tests;

public class TranspositionTests
{
    [Fact]
    public void SamePosition_ProducesSameHash()
    {
        // Two identical boards should produce the same hash
        var engine = CreateEngine();

        var board1 = TestHelpers.StartingBoard();
        var board2 = TestHelpers.StartingBoard();

        // Use reflection or the public GenerateBoardHash via GetBoardScore consistency
        // We'll test indirectly: same board should score the same
        var score1 = engine.GetBoardScore(board1);
        var score2 = engine.GetBoardScore(board2);

        Assert.Equal(score1, score2);
    }

    [Fact]
    public void EmptyBoard_ScoresZero()
    {
        var engine = CreateEngine();
        var board = TestHelpers.EmptyBoard();

        Assert.Equal(0, engine.GetBoardScore(board));
    }

    [Fact]
    public void SinglePiece_ScoreIncludesPositionTable()
    {
        var engine = CreateEngine();

        // Knight at center vs corner
        var boardCenter = TestHelpers.EmptyBoard();
        TestHelpers.PlacePiece(boardCenter, 3, 3, new Knight(true));

        var boardCorner = TestHelpers.EmptyBoard();
        TestHelpers.PlacePiece(boardCorner, 0, 0, new Knight(true));

        var centerScore = engine.GetBoardScore(boardCenter);
        var cornerScore = engine.GetBoardScore(boardCorner);

        // Both positive (max player piece), but center should be higher
        Assert.True(centerScore > 0);
        Assert.True(cornerScore > 0);
        Assert.True(centerScore > cornerScore);
    }

    private static MinMaxEngine CreateEngine()
    {
        return new MinMaxEngine(
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
}
