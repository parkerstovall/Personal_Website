using ChessApi.HelperClasses.Chess;
using ChessApi.Models.API;
using ChessApi.Pieces;

namespace ChessApi.Tests;

public class OrderPossibleMovesTests
{
    private readonly MinMaxEngine _engine;

    public OrderPossibleMovesTests()
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
    public void CapturesAreOrderedBeforeNonCaptures()
    {
        var moves = new List<PossibleMove>
        {
            new()
            {
                MoveFrom = [3, 3],
                MoveTo = [4, 3],
                MovingPiece = new Pawn(true),
                CapturedPiece = null,
            },
            new()
            {
                MoveFrom = [3, 4],
                MoveTo = [4, 5],
                MovingPiece = new Pawn(true),
                CapturedPiece = new Knight(false),
            },
            new()
            {
                MoveFrom = [1, 0],
                MoveTo = [2, 0],
                MovingPiece = new Rook(true),
                CapturedPiece = null,
            },
        };

        _engine.OrderPossibleMoves(moves);

        Assert.NotNull(moves[0].CapturedPiece);
        Assert.Null(moves[1].CapturedPiece);
        Assert.Null(moves[2].CapturedPiece);
    }

    [Fact]
    public void CapturesAreSortedDescendingByValue()
    {
        var moves = new List<PossibleMove>
        {
            new()
            {
                MoveFrom = [3, 3],
                MoveTo = [4, 4],
                MovingPiece = new Pawn(true),
                CapturedPiece = new Pawn(false),
            }, // 100
            new()
            {
                MoveFrom = [3, 4],
                MoveTo = [4, 5],
                MovingPiece = new Pawn(true),
                CapturedPiece = new Queen(false),
            }, // 900
            new()
            {
                MoveFrom = [3, 5],
                MoveTo = [4, 6],
                MovingPiece = new Pawn(true),
                CapturedPiece = new Knight(false),
            }, // 300
        };

        _engine.OrderPossibleMoves(moves);

        Assert.Equal(900, moves[0].CapturedPiece!.Value);
        Assert.Equal(300, moves[1].CapturedPiece!.Value);
        Assert.Equal(100, moves[2].CapturedPiece!.Value);
    }

    [Fact]
    public void NonCapturesAreSortedAscendingByValue()
    {
        var moves = new List<PossibleMove>
        {
            new()
            {
                MoveFrom = [0, 3],
                MoveTo = [1, 3],
                MovingPiece = new Queen(true),
            }, // 900
            new()
            {
                MoveFrom = [0, 1],
                MoveTo = [2, 2],
                MovingPiece = new Knight(true),
            }, // 300
            new()
            {
                MoveFrom = [1, 0],
                MoveTo = [2, 0],
                MovingPiece = new Pawn(true),
            }, // 100
        };

        _engine.OrderPossibleMoves(moves);

        Assert.Equal(100, moves[0].MovingPiece.Value);
        Assert.Equal(300, moves[1].MovingPiece.Value);
        Assert.Equal(900, moves[2].MovingPiece.Value);
    }

    [Fact]
    public void SortsInPlace_DoesNotReturnNewList()
    {
        var moves = new List<PossibleMove>
        {
            new()
            {
                MoveFrom = [0, 3],
                MoveTo = [1, 3],
                MovingPiece = new Queen(true),
            },
            new()
            {
                MoveFrom = [1, 0],
                MoveTo = [2, 0],
                MovingPiece = new Pawn(true),
            },
        };

        var originalRef = moves;
        _engine.OrderPossibleMoves(moves);

        Assert.Same(originalRef, moves);
    }

    [Fact]
    public void HasMovedPiecesGetPriorityOverUnmovedPieces()
    {
        // A pawn on row 4 (has moved) vs a pawn on row 1 (starting row, not moved)
        var moves = new List<PossibleMove>
        {
            new()
            {
                MoveFrom = [1, 0],
                MoveTo = [2, 0],
                MovingPiece = new Pawn(true),
            }, // starting row -> HasMoved=false -> value 100
            new()
            {
                MoveFrom = [4, 0],
                MoveTo = [5, 0],
                MovingPiece = new Pawn(true),
            }, // row 4 -> HasMoved=true -> value 101
        };

        _engine.OrderPossibleMoves(moves);

        // HasMoved pawn (value 101) should sort after non-moved pawn (value 100)
        // Both are pawns so the moved one gets +1, sorting it second in ascending order
        Assert.Equal(1, moves[0].MoveFrom[0]); // starting row first (lower value)
        Assert.Equal(4, moves[1].MoveFrom[0]); // moved pawn second (higher value)
    }
}
