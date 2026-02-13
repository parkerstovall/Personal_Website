using api.models.client;
using ChessApi.Models.API;
using ChessApi.Models.DB;
using ChessApi.Pieces;
using ChessApi.Pieces.Interfaces;

namespace ChessApi.HelperClasses.Chess
{
    internal static class BoardHelper
    {
        internal static Board GetNewBoard()
        {
            string[] pieceOrder =
            {
                "Rook",
                "Knight",
                "Bishop",
                "Queen",
                "King",
                "Bishop",
                "Knight",
                "Rook",
            };

            bool whitePiece = false;
            Board board = new();

            for (int i = 0; i < 8; i++)
            {
                board.Rows.Add(new());
                for (var j = 0; j < 8; j++)
                {
                    IPiece? piece = null;
                    if (i == 0 || i == 7)
                    {
                        piece = PieceFactory.GetPiece(!whitePiece, pieceOrder[j]);
                    }
                    else if (i == 1 || i == 6)
                    {
                        piece = PieceFactory.GetPiece(!whitePiece, "Pawn");
                    }

                    board.Rows[i].Squares.Add(new() { Coords = [i, j], Piece = piece });
                }

                if (i == 1)
                {
                    whitePiece = !whitePiece;
                }
            }

            return board;
        }

        //Don't want to expose the full board class to the client
        internal static BoardDisplay GetBoardForDisplay(Game game)
        {
            BoardDisplay boardDisplay = new();
            bool whiteSquare = true;

            foreach (BoardRow row in game.Board.Rows)
            {
                BoardDisplayRow displayRow = new();

                foreach (BoardSquare square in row.Squares)
                {
                    displayRow.Squares.Add(
                        CreateBoardSquare(
                            whiteSquare,
                            square,
                            game.AvailableMoves,
                            game.SelectedSquare
                        )
                    );
                    whiteSquare = !whiteSquare;
                }

                whiteSquare = !whiteSquare;
                boardDisplay.Rows.Add(displayRow);
            }

            return boardDisplay;
        }

        internal static BoardDisplaySquare CreateBoardSquare(
            bool whiteSquare,
            BoardSquare square,
            List<int[]>? moves,
            int[]? selectedSquare
        )
        {
            string backColor = whiteSquare ? "white" : "black";
            string cssClass = $"boardBtn {backColor}";

            if (square.Piece is not null)
            {
                cssClass += " " + square.Piece.ToString();

                if (
                    selectedSquare is not null
                    && selectedSquare[0] == square.Coords[0]
                    && selectedSquare[1] == square.Coords[1]
                )
                {
                    cssClass += " selected";
                }

                if (square.Piece is King king)
                {
                    if (king.InCheckMate)
                    {
                        cssClass += " inCheckMate";
                    }
                    else if (king.InCheck)
                    {
                        cssClass += " inCheck";
                    }
                }
            }

            if (moves is not null && CheckSquareInMoves(square, moves))
            {
                cssClass += " highlighted";
            }

            return new()
            {
                Row = square.Coords[0],
                Col = square.Coords[1],
                CssClass = cssClass,
            };
        }

        internal static bool CheckSquareInMoves(BoardSquare square, List<int[]> moves)
        {
            foreach (int[] move in moves)
            {
                if (move[0] == square.Coords[0] && move[1] == square.Coords[1])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
