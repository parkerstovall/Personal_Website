using ChessApi.HelperClasses.Chess;
using ChessApi.Models.API;
using ChessApi.Pieces;

namespace ChessApi.Tests;

public class BoardScoringTests
{
    private readonly MinMaxEngine _engine;

    public BoardScoringTests()
    {
        _engine = new MinMaxEngine(
            new MinMaxEngineOptions
            {
                MaxDepth = 1,
                MaxTime = null,
                MaxPlayer = true, // black is maximizing
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
    public void StartingPosition_ScoresZero()
    {
        var board = TestHelpers.StartingBoard();
        var score = _engine.GetBoardScore(board);

        Assert.Equal(0, score);
    }

    [Fact]
    public void ExtraPiece_IncreasesScore()
    {
        var board = TestHelpers.EmptyBoard();
        // Single black queen (max player)
        TestHelpers.PlacePiece(board, 4, 4, new Queen(true));

        var score = _engine.GetBoardScore(board);

        Assert.True(score > 0);
    }

    [Fact]
    public void ExtraEnemyPiece_DecreasesScore()
    {
        var board = TestHelpers.EmptyBoard();
        // Single white queen (min player)
        TestHelpers.PlacePiece(board, 4, 4, new Queen(false));

        var score = _engine.GetBoardScore(board);

        Assert.True(score < 0);
    }

    [Fact]
    public void PieceSquareTablesAffectScore()
    {
        var board1 = TestHelpers.EmptyBoard();
        var board2 = TestHelpers.EmptyBoard();

        // Knight in center vs corner — center should score higher
        TestHelpers.PlacePiece(board1, 4, 4, new Knight(true));
        TestHelpers.PlacePiece(board2, 0, 0, new Knight(true));

        var scoreCenter = _engine.GetBoardScore(board1);
        var scoreCorner = _engine.GetBoardScore(board2);

        Assert.True(scoreCenter > scoreCorner);
    }

    [Fact]
    public void CapturingPiece_ChangesScoreByPieceValue()
    {
        var board = TestHelpers.EmptyBoard();

        // Black queen and white knight
        TestHelpers.PlacePiece(board, 4, 4, new Queen(true));
        TestHelpers.PlacePiece(board, 2, 3, new Knight(false));

        var scoreBefore = _engine.GetBoardScore(board);

        // Remove the white knight (simulate capture)
        board.Rows[2].Squares[3].Piece = null;

        var scoreAfter = _engine.GetBoardScore(board);

        // Score should increase (enemy piece removed)
        Assert.True(scoreAfter > scoreBefore);
    }
}
