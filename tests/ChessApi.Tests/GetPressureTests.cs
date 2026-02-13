using ChessApi.Models.API;
using ChessApi.Pieces;

namespace ChessApi.Tests;

public class GetPressureTests
{
    [Fact]
    public void Queen_GetPressure_ReturnsCoordsReferences()
    {
        var board = TestHelpers.EmptyBoard();
        var queen = new Queen(true);
        TestHelpers.PlacePiece(board, 4, 4, queen);

        var pressure = queen.GetPressure(board, [4, 4]);

        // Every returned coordinate should be the same reference as the board square's Coords
        foreach (var coords in pressure)
        {
            var square = board.Rows[coords[0]].Squares[coords[1]];
            Assert.Same(square.Coords, coords);
        }
    }

    [Fact]
    public void Rook_GetPressure_ReturnsCoordsReferences()
    {
        var board = TestHelpers.EmptyBoard();
        var rook = new Rook(true);
        TestHelpers.PlacePiece(board, 4, 4, rook);

        var pressure = rook.GetPressure(board, [4, 4]);

        foreach (var coords in pressure)
        {
            var square = board.Rows[coords[0]].Squares[coords[1]];
            Assert.Same(square.Coords, coords);
        }
    }

    [Fact]
    public void Bishop_GetPressure_ReturnsCoordsReferences()
    {
        var board = TestHelpers.EmptyBoard();
        var bishop = new Bishop(true);
        TestHelpers.PlacePiece(board, 4, 4, bishop);

        var pressure = bishop.GetPressure(board, [4, 4]);

        foreach (var coords in pressure)
        {
            var square = board.Rows[coords[0]].Squares[coords[1]];
            Assert.Same(square.Coords, coords);
        }
    }

    [Fact]
    public void Knight_GetPressure_ReturnsCoordsReferences()
    {
        var board = TestHelpers.EmptyBoard();
        var knight = new Knight(true);
        TestHelpers.PlacePiece(board, 4, 4, knight);

        var pressure = knight.GetPressure(board, [4, 4]);

        foreach (var coords in pressure)
        {
            var square = board.Rows[coords[0]].Squares[coords[1]];
            Assert.Same(square.Coords, coords);
        }
    }

    [Fact]
    public void King_GetPressure_ReturnsCoordsReferences()
    {
        var board = TestHelpers.EmptyBoard();
        var king = new King(true);
        TestHelpers.PlacePiece(board, 4, 4, king);

        var pressure = king.GetPressure(board, [4, 4]);

        foreach (var coords in pressure)
        {
            var square = board.Rows[coords[0]].Squares[coords[1]];
            Assert.Same(square.Coords, coords);
        }
    }

    [Fact]
    public void Pawn_GetPressure_ReturnsCoordsReferences()
    {
        var board = TestHelpers.EmptyBoard();
        var pawn = new Pawn(true);
        TestHelpers.PlacePiece(board, 4, 4, pawn);

        var pressure = pawn.GetPressure(board, [4, 4]);

        foreach (var coords in pressure)
        {
            var square = board.Rows[coords[0]].Squares[coords[1]];
            Assert.Same(square.Coords, coords);
        }
    }

    [Fact]
    public void Queen_GetPressure_CorrectSquareCount_EmptyBoard()
    {
        var board = TestHelpers.EmptyBoard();
        var queen = new Queen(true);
        TestHelpers.PlacePiece(board, 4, 4, queen);

        var pressure = queen.GetPressure(board, [4, 4]);

        // From center, queen sees: 7 up/down + 7 left/right + 7+7 diagonals = 27 squares
        // Actually: right 3, left 4, down 3, up 4, diag: 3+3+3+4 = 27
        Assert.Equal(27, pressure.Count);
    }

    [Fact]
    public void Rook_GetPressure_CorrectSquareCount_EmptyBoard()
    {
        var board = TestHelpers.EmptyBoard();
        var rook = new Rook(true);
        TestHelpers.PlacePiece(board, 4, 4, rook);

        var pressure = rook.GetPressure(board, [4, 4]);

        // From (4,4): right 3, left 4, down 3, up 4 = 14
        Assert.Equal(14, pressure.Count);
    }

    [Fact]
    public void Knight_GetPressure_CorrectSquareCount_Center()
    {
        var board = TestHelpers.EmptyBoard();
        var knight = new Knight(true);
        TestHelpers.PlacePiece(board, 4, 4, knight);

        var pressure = knight.GetPressure(board, [4, 4]);

        // Knight in center has 8 possible squares
        Assert.Equal(8, pressure.Count);
    }

    [Fact]
    public void Knight_GetPressure_CorrectSquareCount_Corner()
    {
        var board = TestHelpers.EmptyBoard();
        var knight = new Knight(true);
        TestHelpers.PlacePiece(board, 0, 0, knight);

        var pressure = knight.GetPressure(board, [0, 0]);

        // Knight in corner: only (2,1) and (1,2) are on board
        Assert.Equal(2, pressure.Count);
    }

    [Fact]
    public void King_GetPressure_CorrectSquareCount_Center()
    {
        var board = TestHelpers.EmptyBoard();
        var king = new King(true);
        TestHelpers.PlacePiece(board, 4, 4, king);

        var pressure = king.GetPressure(board, [4, 4]);

        Assert.Equal(8, pressure.Count);
    }

    [Fact]
    public void Pawn_GetPressure_CorrectSquareCount()
    {
        var board = TestHelpers.EmptyBoard();
        var pawn = new Pawn(true); // black pawn, moves down (dir = +1)
        TestHelpers.PlacePiece(board, 4, 4, pawn);

        var pressure = pawn.GetPressure(board, [4, 4]);

        // Pawn attacks 2 diagonal squares
        Assert.Equal(2, pressure.Count);
    }

    [Fact]
    public void SlidingPiece_GetPressure_StopsAtBlockingPiece()
    {
        var board = TestHelpers.EmptyBoard();
        var rook = new Rook(true);
        var blocker = new Pawn(true);
        TestHelpers.PlacePiece(board, 4, 4, rook);
        TestHelpers.PlacePiece(board, 4, 6, blocker); // blocks rightward after 1 square

        var pressure = rook.GetPressure(board, [4, 4]);

        // Right: (4,5), (4,6) = 2 (stops at blocker, includes it)
        // Left: (4,3), (4,2), (4,1), (4,0) = 4
        // Down: (5,4), (6,4), (7,4) = 3
        // Up: (3,4), (2,4), (1,4), (0,4) = 4
        Assert.Equal(13, pressure.Count);
    }
}
