using System.ComponentModel;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using ChessApi.Models.API;
using ChessApi.Models.DB;
using ChessApi.Pieces;
using ChessApi.Pieces.Interfaces;

namespace ChessApi.HelperClasses.Chess
{
    internal static class MoveHelper
    {
        internal static List<PossibleMove> GetMovesFromPiece(
            Board board,
            int[] clickedSquare,
            bool? checkColor
        )
        {
            BoardSquare square = board.Rows[clickedSquare[0]].Squares[clickedSquare[1]];
            List<PossibleMove> moves =
                square.Piece?.GetPaths(board, clickedSquare, checkColor == square.Piece?.Color)
                ?? [];

            return moves;
        }

        internal static bool TryMovePiece(
            int[] coords,
            int[] start,
            List<int[]> moves,
            ref Game game
        )
        {
            foreach (int[] move in moves)
            {
                if (move[0] == coords[0] && move[1] == coords[1])
                {
                    MovePiece(start, move, ref game);
                    return true;
                }
            }

            return false;
        }

        internal static void MovePiece(int[] start, int[] dest, ref Game game)
        {
            BoardSquare from = game.Board.Rows[start[0]].Squares[start[1]];
            BoardSquare to = game.Board.Rows[dest[0]].Squares[dest[1]];

            if (from.Piece is null)
            {
                return;
            }

            //Attempts EnPassant if valid
            CheckEnPassantCapture(from.Piece, to, start, dest, ref game);

            //Set all values to empty
            ResetBoard(ref game);

            //Check if EnPassant is available for next turn
            CheckEnPassantMove(from.Piece, start, dest, ref game);

            //Actual Piece move
            MovePieceInner(to, from, ref game);

            //Refresh board stats
            CheckTracker tracker = RefreshBoard(ref game);

            //Checks for Pins
            CheckPins(tracker, ref game);

            //Check for Checkmate
            CheckCheckmate(tracker, ref game);

            game.CheckedColor = tracker.CheckColor();
        }

        private static void CheckEnPassantCapture(
            IPiece from,
            BoardSquare to,
            int[] start,
            int[] dest,
            ref Game game
        )
        {
            //Checking for en passant capture
            if (
                from is Pawn pawn
                && to.Piece is null
                && to.EnPassantColor.HasValue
                && to.EnPassantColor != pawn.Color
            )
            {
                int[] enPassantSquare = { dest[0], dest[1] };
                enPassantSquare[0] += (start[0] > dest[0]) ? 1 : -1;
                game.Board.Rows[enPassantSquare[0]].Squares[enPassantSquare[1]].Piece = null;
            }
        }

        private static void ResetBoard(ref Game game)
        {
            foreach (BoardRow row in game.Board.Rows)
            {
                foreach (BoardSquare square in row.Squares)
                {
                    square.CheckBlockingColor = null;
                    square.EnPassantColor = null;
                    square.BlackPressure = 0;
                    square.WhitePressure = 0;
                    square.WhiteKingPressure = false;
                    square.BlackKingPressure = false;

                    if (square.Piece is not null)
                    {
                        square.Piece.PinnedDir = Direction.None;

                        if (square.Piece is King king)
                        {
                            king.InCheck = false;
                        }
                    }
                }
            }
        }

        private static void CheckEnPassantMove(IPiece piece, int[] start, int[] dest, ref Game game)
        {
            //Checking for en passant opportunity
            if (Math.Abs(start[0] - dest[0]) == 2 && piece is Pawn pawn)
            {
                int[] enPassantSquare = { start[0], start[1] };
                enPassantSquare[0] += (start[0] > dest[0]) ? -1 : 1;
                game.Board.Rows[enPassantSquare[0]].Squares[enPassantSquare[1]].EnPassantColor =
                    pawn.Color;
            }
        }

        private static void MovePieceInner(BoardSquare to, BoardSquare from, ref Game game)
        {
            to.Piece = from.Piece;
            from.Piece = null;

            if (to.Piece is IPieceHasMoved pieceHasMoved)
            {
                pieceHasMoved.HasMoved = true;

                if (pieceHasMoved is Pawn pawn)
                {
                    if (to.Coords[0] == 0 || to.Coords[0] == 7)
                    {
                        to.Piece = new Queen(pawn.Color);
                    }
                }
            }

            if (Math.Abs(from.Coords[1] - to.Coords[1]) > 1 && to.Piece is King)
            {
                CastleKing(from.Coords, to.Coords, ref game);
            }
        }

        private static void CastleKing(int[] start, int[] dest, ref Game game)
        {
            int rookStartCol = start[1] > dest[1] ? 0 : 7;
            int rookDestCol = start[1] > dest[1] ? 3 : 5;

            BoardSquare rookFrom = game.Board.Rows[start[0]].Squares[rookStartCol];
            BoardSquare rookTo = game.Board.Rows[start[0]].Squares[rookDestCol];

            rookTo.Piece = rookFrom.Piece;
            rookFrom.Piece = null;

            if (rookTo.Piece is IPieceHasMoved pieceHasMoved)
            {
                pieceHasMoved.HasMoved = true;
            }
        }

        private static CheckTracker RefreshBoard(ref Game game)
        {
            CheckTracker tracker = new();
            BoardSquare? checkSquare = null;
            int[] kingLoc = [-1, -1];

            foreach (BoardRow row in game.Board.Rows)
            {
                foreach (BoardSquare square in row.Squares)
                {
                    AddBoardPressure(square, ref game, ref tracker, ref checkSquare, ref kingLoc);
                }
            }

            if (checkSquare is not null)
            {
                if (checkSquare.Piece is IPieceCanPin checkSavingSquaresPiece)
                {
                    tracker.SetHasSavingSquares(
                        !checkSavingSquaresPiece.Color,
                        checkSavingSquaresPiece.HasSavingSquares(
                            [checkSquare.Coords[0], checkSquare.Coords[1]],
                            kingLoc,
                            ref game
                        )
                    );
                }
                else if (checkSquare.Piece is IPieceDirectAttacker directAttacker)
                {
                    var pressure = !directAttacker.Color
                        ? checkSquare.WhitePressure
                        : checkSquare.BlackPressure;
                    var enemyPressure = !directAttacker.Color
                        ? checkSquare.BlackPressure
                        : checkSquare.WhitePressure;
                    var isFromKing = !directAttacker.Color
                        ? checkSquare.BlackKingPressure
                        : checkSquare.WhiteKingPressure;

                    if (enemyPressure > 1 || (enemyPressure == 1 && (!isFromKing || pressure == 0)))
                    {
                        checkSquare.CheckBlockingColor = directAttacker.Color;
                        tracker.SetHasSavingSquares(directAttacker.Color, true);
                    }
                }
            }

            return tracker;
        }

        private static void AddBoardPressure(
            BoardSquare square,
            ref Game game,
            ref CheckTracker tracker,
            ref BoardSquare? checkSquare,
            ref int[] kingLoc
        )
        {
            if (square.Piece is null)
            {
                return;
            }

            if (square.Piece is IPieceCanPin)
            {
                tracker.PinPieces.Add(square);
            }

            if (square.Piece is King)
            {
                tracker.SetKing(square);
            }

            foreach (int[] pMove in square.Piece.GetPressure(game.Board, square.Coords))
            {
                BoardSquare pSquare = game.Board.Rows[pMove[0]].Squares[pMove[1]];

                if (pSquare.Piece is King king)
                {
                    if (square.Piece.Color != king.Color)
                    {
                        if (
                            (square.Piece is IPieceCanPin || square.Piece is IPieceDirectAttacker)
                            && tracker.GetKingAttackers(king.Color).Count == 0
                        )
                        {
                            checkSquare = square;
                            kingLoc = pMove;
                        }
                        else
                        {
                            checkSquare = null;
                            kingLoc = [-1, -1];
                            tracker.SetHasSavingSquares(king.Color, false);
                        }

                        tracker.AddAttacker(square);
                        tracker.SetKing(pSquare);
                        king.InCheck = true;
                    }
                }

                if (!square.Piece.Color)
                {
                    if (square.Piece is King)
                    {
                        pSquare.WhiteKingPressure = true;
                    }

                    pSquare.WhitePressure++;
                }
                else
                {
                    if (square.Piece is King)
                    {
                        pSquare.BlackKingPressure = true;
                    }

                    pSquare.BlackPressure++;
                }
            }
        }

        private static void CheckPins(CheckTracker tracker, ref Game game)
        {
            foreach (BoardSquare square in tracker.PinPieces)
            {
                if (square.Piece is null || square.Piece is not IPieceCanPin piece)
                {
                    continue;
                }

                int[] lStart = [square.Coords[0], square.Coords[1]];
                if (!piece.Color && tracker.BlackKing is not null)
                {
                    piece.CheckPins(lStart, tracker.BlackKing.Coords, ref game);
                }
                else if (tracker.WhiteKing is not null)
                {
                    piece.CheckPins(lStart, tracker.WhiteKing.Coords, ref game);
                }
            }
        }

        private static void CheckCheckmate(CheckTracker tracker, ref Game game)
        {
            if (tracker.WhiteKing is not null)
            {
                CheckKingInCheckMate(tracker.WhiteKing, tracker, ref game);
            }
            else if (tracker.BlackKing is not null)
            {
                CheckKingInCheckMate(tracker.BlackKing, tracker, ref game);
            }
        }

        private static void CheckKingInCheckMate(
            BoardSquare square,
            CheckTracker tracker,
            ref Game game
        )
        {
            if (square.Piece is null || square.Piece is not King king || !king.InCheck)
            {
                return;
            }

            var attackers = tracker.GetKingAttackers(king.Color);
            if (attackers.Count == 1)
            {
                var attacker = attackers[0];
                var pressure = !king.Color ? attacker.WhitePressure : attacker.BlackPressure;
                var enemyPressure = !king.Color ? attacker.BlackPressure : attacker.WhitePressure;
                var isFromKing = !king.Color
                    ? attacker.WhiteKingPressure
                    : attacker.BlackKingPressure;

                if (pressure > 1 || (pressure == 1 && (!isFromKing || enemyPressure == 0)))
                {
                    return;
                }
            }
            else
            {
                foreach (var attacker in attackers)
                {
                    var pressure = !king.Color ? attacker.WhitePressure : attacker.BlackPressure;
                    var enemyPressure = !king.Color
                        ? attacker.BlackPressure
                        : attacker.WhitePressure;
                    var isFromKing = !king.Color
                        ? attacker.WhiteKingPressure
                        : attacker.BlackKingPressure;

                    if (pressure == 1 && (!isFromKing || enemyPressure == 0))
                    {
                        return;
                    }
                }
            }

            if (king.GetPaths(game.Board, square.Coords, true).Count > 0)
            {
                return;
            }

            if (!king.Color ? tracker.HasWhiteSavingSquares : tracker.HasBlackSavingSquares)
            {
                return;
            }

            king.InCheckMate = true;
            game.SetStatus(!king.Color ? GameStatus.CheckMateWhite : GameStatus.CheckMateBlack);
        }
    }
}
