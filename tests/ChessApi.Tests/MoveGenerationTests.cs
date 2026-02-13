using ChessApi.HelperClasses.Chess;
using ChessApi.Models.API;
using ChessApi.Pieces;

namespace ChessApi.Tests;

public class MoveGenerationTests
{
    // --- Pawn ---

    [Fact]
    public void Pawn_CanMoveForwardOne()
    {
        var board = TestHelpers.EmptyBoard();
        var pawn = new Pawn(true) { HasMoved = true }; // black pawn, moves down (dir = +1)
        TestHelpers.PlacePiece(board, 4, 4, pawn);

        var moves = pawn.GetPaths(board, [4, 4], false);

        Assert.Contains(moves, m => m.MoveTo[0] == 5 && m.MoveTo[1] == 4);
    }

    [Fact]
    public void Pawn_CanMoveForwardTwo_FromStartingRow()
    {
        var board = TestHelpers.EmptyBoard();
        var pawn = new Pawn(true); // black pawn on starting row
        TestHelpers.PlacePiece(board, 1, 4, pawn);

        var moves = pawn.GetPaths(board, [1, 4], false);

        Assert.Contains(moves, m => m.MoveTo[0] == 2 && m.MoveTo[1] == 4);
        Assert.Contains(moves, m => m.MoveTo[0] == 3 && m.MoveTo[1] == 4);
    }

    [Fact]
    public void Pawn_CannotMoveForwardTwo_AfterMoving()
    {
        var board = TestHelpers.EmptyBoard();
        var pawn = new Pawn(true) { HasMoved = true };
        TestHelpers.PlacePiece(board, 3, 4, pawn);

        var moves = pawn.GetPaths(board, [3, 4], false);

        Assert.DoesNotContain(moves, m => m.MoveTo[0] == 5 && m.MoveTo[1] == 4);
    }

    [Fact]
    public void Pawn_CanCaptureDiagonally()
    {
        var board = TestHelpers.EmptyBoard();
        var pawn = new Pawn(true) { HasMoved = true }; // black pawn
        var enemy = new Pawn(false); // white
        TestHelpers.PlacePiece(board, 4, 4, pawn);
        TestHelpers.PlacePiece(board, 5, 3, enemy);

        var moves = pawn.GetPaths(board, [4, 4], false);

        var capture = Assert.Single(moves, m => m.CapturedPiece is not null);
        Assert.Equal(5, capture.MoveTo[0]);
        Assert.Equal(3, capture.MoveTo[1]);
    }

    [Fact]
    public void Pawn_CannotMoveForward_WhenBlocked()
    {
        var board = TestHelpers.EmptyBoard();
        var pawn = new Pawn(true) { HasMoved = true };
        var blocker = new Pawn(false);
        TestHelpers.PlacePiece(board, 4, 4, pawn);
        TestHelpers.PlacePiece(board, 5, 4, blocker);

        var moves = pawn.GetPaths(board, [4, 4], false);

        Assert.DoesNotContain(moves, m => m.MoveTo[0] == 5 && m.MoveTo[1] == 4);
    }

    // --- Knight ---

    [Fact]
    public void Knight_Has8Moves_FromCenter()
    {
        var board = TestHelpers.EmptyBoard();
        var knight = new Knight(true);
        TestHelpers.PlacePiece(board, 4, 4, knight);

        var moves = knight.GetPaths(board, [4, 4], false);

        Assert.Equal(8, moves.Count);
    }

    [Fact]
    public void Knight_Has2Moves_FromCorner()
    {
        var board = TestHelpers.EmptyBoard();
        var knight = new Knight(true);
        TestHelpers.PlacePiece(board, 0, 0, knight);

        var moves = knight.GetPaths(board, [0, 0], false);

        Assert.Equal(2, moves.Count);
    }

    [Fact]
    public void Knight_CannotMoveWhenPinned()
    {
        var board = TestHelpers.EmptyBoard();
        var knight = new Knight(true) { PinnedDir = Direction.FromTopToBottom };
        TestHelpers.PlacePiece(board, 4, 4, knight);

        var moves = knight.GetPaths(board, [4, 4], false);

        Assert.Empty(moves);
    }

    [Fact]
    public void Knight_CapturesEnemyPiece()
    {
        var board = TestHelpers.EmptyBoard();
        var knight = new Knight(true);
        var enemy = new Pawn(false);
        TestHelpers.PlacePiece(board, 4, 4, knight);
        TestHelpers.PlacePiece(board, 2, 3, enemy); // valid L-shape

        var moves = knight.GetPaths(board, [4, 4], false);
        var capture = moves.First(m => m.MoveTo[0] == 2 && m.MoveTo[1] == 3);

        Assert.Same(enemy, capture.CapturedPiece);
    }

    [Fact]
    public void Knight_CannotCaptureFriendly()
    {
        var board = TestHelpers.EmptyBoard();
        var knight = new Knight(true);
        var friendly = new Pawn(true);
        TestHelpers.PlacePiece(board, 4, 4, knight);
        TestHelpers.PlacePiece(board, 2, 3, friendly);

        var moves = knight.GetPaths(board, [4, 4], false);

        Assert.DoesNotContain(moves, m => m.MoveTo[0] == 2 && m.MoveTo[1] == 3);
    }

    // --- Bishop ---

    [Fact]
    public void Bishop_SlidesAlongDiagonals()
    {
        var board = TestHelpers.EmptyBoard();
        var bishop = new Bishop(true);
        TestHelpers.PlacePiece(board, 4, 4, bishop);

        var moves = bishop.GetPaths(board, [4, 4], false);

        // From center: 4 diagonals, total squares = 3+3+3+4 = 13
        Assert.Equal(13, moves.Count);
    }

    [Fact]
    public void Bishop_StopsAtFriendlyPiece()
    {
        var board = TestHelpers.EmptyBoard();
        var bishop = new Bishop(true);
        var friendly = new Pawn(true);
        TestHelpers.PlacePiece(board, 4, 4, bishop);
        TestHelpers.PlacePiece(board, 6, 6, friendly); // blocks one diagonal

        var moves = bishop.GetPaths(board, [4, 4], false);

        Assert.DoesNotContain(moves, m => m.MoveTo[0] == 6 && m.MoveTo[1] == 6);
        Assert.Contains(moves, m => m.MoveTo[0] == 5 && m.MoveTo[1] == 5);
    }

    // --- Rook ---

    [Fact]
    public void Rook_SlidesAlongRanksAndFiles()
    {
        var board = TestHelpers.EmptyBoard();
        var rook = new Rook(true);
        TestHelpers.PlacePiece(board, 4, 4, rook);

        var moves = rook.GetPaths(board, [4, 4], false);

        // From center: 3+4+3+4 = 14
        Assert.Equal(14, moves.Count);
    }

    [Fact]
    public void Rook_CapturesEnemyAndStops()
    {
        var board = TestHelpers.EmptyBoard();
        var rook = new Rook(true);
        var enemy = new Knight(false);
        TestHelpers.PlacePiece(board, 4, 4, rook);
        TestHelpers.PlacePiece(board, 4, 6, enemy);

        var moves = rook.GetPaths(board, [4, 4], false);

        // Can reach (4,5) and capture at (4,6), but not (4,7)
        Assert.Contains(
            moves,
            m => m.MoveTo[0] == 4 && m.MoveTo[1] == 6 && m.CapturedPiece == enemy
        );
        Assert.DoesNotContain(moves, m => m.MoveTo[0] == 4 && m.MoveTo[1] == 7);
    }

    // --- Queen ---

    [Fact]
    public void Queen_CombinesBishopAndRookMoves()
    {
        var board = TestHelpers.EmptyBoard();
        var queen = new Queen(true);
        TestHelpers.PlacePiece(board, 4, 4, queen);

        var moves = queen.GetPaths(board, [4, 4], false);

        // Rook: 14, Bishop: 13 = 27
        Assert.Equal(27, moves.Count);
    }

    // --- King ---

    [Fact]
    public void King_MovesOneSquareInAllDirections()
    {
        var board = TestHelpers.EmptyBoard();
        var king = new King(true) { HasMoved = true }; // HasMoved to suppress castle moves
        TestHelpers.PlacePiece(board, 4, 4, king);

        var moves = king.GetPaths(board, [4, 4], false);

        Assert.Equal(8, moves.Count);
        foreach (var m in moves)
        {
            Assert.True(Math.Abs(m.MoveTo[0] - 4) <= 1);
            Assert.True(Math.Abs(m.MoveTo[1] - 4) <= 1);
        }
    }

    [Fact]
    public void King_CannotMoveIntoPressure()
    {
        var board = TestHelpers.EmptyBoard();
        var king = new King(true); // black king
        var enemyRook = new Rook(false); // white rook
        TestHelpers.PlacePiece(board, 4, 4, king);
        TestHelpers.PlacePiece(board, 0, 5, enemyRook);

        // Calculate pressure so the king knows col 5 is under attack
        foreach (var pMove in enemyRook.GetPressure(board, [0, 5]))
        {
            board.Rows[pMove[0]].Squares[pMove[1]].WhitePressure++;
        }

        var moves = king.GetPaths(board, [4, 4], false);

        // Column 5 should be excluded due to white pressure
        Assert.DoesNotContain(moves, m => m.MoveTo[1] == 5);
    }
}
