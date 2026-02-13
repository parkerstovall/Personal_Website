using ChessApi.HelperClasses.Chess;
using ChessApi.Models.API;
using ChessApi.Models.DB;
using ChessApi.Pieces;
using ChessApi.Pieces.Interfaces;

namespace ChessApi.Tests;

internal static class TestHelpers
{
    /// <summary>
    /// Creates an empty 8x8 board with no pieces.
    /// </summary>
    internal static Board EmptyBoard()
    {
        Board board = new();
        for (int i = 0; i < 8; i++)
        {
            board.Rows.Add(new());
            for (int j = 0; j < 8; j++)
            {
                board.Rows[i].Squares.Add(new() { Coords = [i, j] });
            }
        }
        return board;
    }

    /// <summary>
    /// Creates a standard starting board via BoardHelper.
    /// </summary>
    internal static Board StartingBoard()
    {
        return BoardHelper.GetNewBoard();
    }

    /// <summary>
    /// Creates a Game with a starting board position.
    /// </summary>
    internal static Game NewGame()
    {
        return new Game { Board = StartingBoard() };
    }

    /// <summary>
    /// Creates a Game with an empty board.
    /// </summary>
    internal static Game EmptyGame()
    {
        return new Game { Board = EmptyBoard() };
    }

    /// <summary>
    /// Places a piece on the board at the given coordinates.
    /// </summary>
    internal static void PlacePiece(Board board, int row, int col, IPiece piece)
    {
        board.Rows[row].Squares[col].Piece = piece;
    }

    /// <summary>
    /// Gets the piece at the given coordinates.
    /// </summary>
    internal static IPiece? GetPiece(Board board, int row, int col)
    {
        return board.Rows[row].Squares[col].Piece;
    }
}
