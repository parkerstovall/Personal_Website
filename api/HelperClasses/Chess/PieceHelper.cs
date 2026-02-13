using ChessApi.Models.API;
using ChessApi.Models.DB;
using ChessApi.Pieces;
using ChessApi.Pieces.Interfaces;

namespace ChessApi.HelperClasses.Chess
{
    internal static class PieceHelper
    {
        private static readonly int[] EmptyInc = Array.Empty<int>();

        // Diagonal directions
        private static readonly int[] DiagColTLBR = { 1, -1 };
        private static readonly int[] DiagRowTLBR = { 1, -1 };
        private static readonly int[] DiagColBLTR = { -1, 1 };
        private static readonly int[] DiagRowBLTR = { 1, -1 };

        // Straight directions
        private static readonly int[] StraightColTBBT = { -1, 1 };
        private static readonly int[] StraightRowTBBT = { 0, 0 };
        private static readonly int[] StraightColLRRL = { 0, 0 };
        private static readonly int[] StraightRowLRRL = { -1, 1 };

        // Combined (diag + straight = queen/king)
        private static readonly int[] DiagStraightCol = { -1, -1, 1, 1, -1, 1, 0, 0 };
        private static readonly int[] DiagStraightRow = { 1, -1, -1, 1, 0, 0, -1, 1 };

        // Diag only (bishop)
        private static readonly int[] DiagOnlyCol = { -1, -1, 1, 1 };
        private static readonly int[] DiagOnlyRow = { 1, -1, -1, 1 };

        // Straight only (rook)
        private static readonly int[] StraightOnlyCol = { -1, 1, 0, 0 };
        private static readonly int[] StraightOnlyRow = { 0, 0, -1, 1 };

        internal static bool IsInBoard(int col, int row)
        {
            return col > -1 && col < 8 && row > -1 && row < 8;
        }

        internal static Direction GetDirection(int[] start, int[] dest)
        {
            if (start[0] == dest[0])
            {
                if (start[1] < dest[1])
                {
                    return Direction.FromLeftToRight;
                }
                else
                {
                    return Direction.FromRightToLeft;
                }
            }

            if (start[1] == dest[1])
            {
                if (start[0] < dest[0])
                {
                    return Direction.FromTopToBottom;
                }
                else
                {
                    return Direction.FromBottomToTop;
                }
            }

            int colDiff = start[0] - dest[0];
            int rowDiff = start[1] - dest[1];

            if (colDiff == rowDiff)
            {
                if (colDiff > 0)
                {
                    return Direction.FromBottomRightToTopLeft;
                }
                else
                {
                    return Direction.FromTopLeftToBottomRight;
                }
            }

            if (Math.Abs(colDiff) == Math.Abs(rowDiff))
            {
                if (colDiff > 0)
                {
                    return Direction.FromBottomLeftToTopRight;
                }
                else
                {
                    return Direction.FromTopRightToBottomLeft;
                }
            }

            return Direction.None;
        }

        internal static Tuple<int[], int[]> GetIncrements(
            Direction PinnedDir,
            bool diag,
            bool straight
        )
        {
            if (
                diag && PinnedDir == Direction.FromTopLeftToBottomRight
                || PinnedDir == Direction.FromBottomRightToTopLeft
            )
            {
                return Tuple.Create(DiagColTLBR, DiagRowTLBR);
            }

            if (
                diag && PinnedDir == Direction.FromBottomLeftToTopRight
                || PinnedDir == Direction.FromTopRightToBottomLeft
            )
            {
                return Tuple.Create(DiagColBLTR, DiagRowBLTR);
            }

            if (
                straight && PinnedDir == Direction.FromTopToBottom
                || PinnedDir == Direction.FromBottomToTop
            )
            {
                return Tuple.Create(StraightColTBBT, StraightRowTBBT);
            }

            if (
                straight && PinnedDir == Direction.FromLeftToRight
                || PinnedDir == Direction.FromRightToLeft
            )
            {
                return Tuple.Create(StraightColLRRL, StraightRowLRRL);
            }

            if (PinnedDir == Direction.None)
            {
                if (diag && straight)
                {
                    return Tuple.Create(DiagStraightCol, DiagStraightRow);
                }

                if (diag)
                {
                    return Tuple.Create(DiagOnlyCol, DiagOnlyRow);
                }

                if (straight)
                {
                    return Tuple.Create(StraightOnlyCol, StraightOnlyRow);
                }
            }

            return Tuple.Create(EmptyInc, EmptyInc);
        }

        private static readonly int[] IncTLBR = { 1, 1 };
        private static readonly int[] IncTRBL = { 1, -1 };
        private static readonly int[] IncBRTL = { -1, -1 };
        private static readonly int[] IncBLTR = { -1, 1 };
        private static readonly int[] IncLR = { 0, 1 };
        private static readonly int[] IncRL = { 0, -1 };
        private static readonly int[] IncTB = { 1, 0 };
        private static readonly int[] IncBT = { -1, 0 };
        private static readonly int[] IncNone = { 0, 0 };

        internal static int[] GetSingleIncrement(Direction dir)
        {
            return dir switch
            {
                Direction.FromTopLeftToBottomRight => IncTLBR,
                Direction.FromTopRightToBottomLeft => IncTRBL,
                Direction.FromBottomRightToTopLeft => IncBRTL,
                Direction.FromBottomLeftToTopRight => IncBLTR,
                Direction.FromLeftToRight => IncLR,
                Direction.FromRightToLeft => IncRL,
                Direction.FromTopToBottom => IncTB,
                Direction.FromBottomToTop => IncBT,
                _ => IncNone,
            };
        }

        public static void SetPins(int[] start, int[] inc, Direction dir, bool color, ref Game game)
        {
            start[0] += inc[0];
            start[1] += inc[1];
            IPiece? candidate = null;
            while (IsInBoard(start[0], start[1]))
            {
                BoardSquare square = game.Board.Rows[start[0]].Squares[start[1]];
                if (square.Piece is not null)
                {
                    if (square.Piece.Color == color || square.Piece is King)
                    {
                        break;
                    }

                    if (candidate is not null)
                    {
                        // Second enemy piece found — no pin
                        return;
                    }

                    candidate = square.Piece;
                }

                start[0] += inc[0];
                start[1] += inc[1];
            }

            if (candidate is not null)
            {
                candidate.PinnedDir = dir;
            }
        }

        public static bool SetSavingSquares(int[] start, int[] inc, bool color, ref Game game)
        {
            bool canSave = false;

            while (IsInBoard(start[0], start[1]))
            {
                BoardSquare square = game.Board.Rows[start[0]].Squares[start[1]];

                if (square.Piece is not null && square.Piece is King)
                {
                    break;
                }

                int pawnInc = color == false ? -1 : 1;

                if (IsInBoard(start[0] + pawnInc, start[1]))
                {
                    BoardSquare pawnSquare = game.Board.Rows[start[0] + pawnInc].Squares[start[1]];
                    if (
                        pawnSquare.Piece is not null
                        && pawnSquare.Piece is Pawn pawn
                        && pawn.Color != color
                    )
                    {
                        canSave = true;
                        square.CheckBlockingColor = !color;
                    }
                }

                if (IsInBoard(start[0] + (2 * pawnInc), start[1]))
                {
                    BoardSquare pawnSquare = game.Board.Rows[start[0] + (2 * pawnInc)].Squares[
                        start[1]
                    ];
                    if (
                        pawnSquare.Piece is not null
                        && pawnSquare.Piece is Pawn pawn
                        && !pawn.HasMoved
                        && pawn.Color != color
                    )
                    {
                        canSave = true;
                        square.CheckBlockingColor = !color;
                    }
                }

                if (GetEnemyPressure(color, square) > 1)
                {
                    canSave = true;
                    square.CheckBlockingColor = !color;
                }
                else if (GetEnemyPressure(color, square) == 1 && !GetKingPressure(color, square))
                {
                    canSave = true;
                    square.CheckBlockingColor = !color;
                }

                start[0] += inc[0];
                start[1] += inc[1];
            }

            return canSave;
        }

        private static int GetEnemyPressure(bool color, BoardSquare square)
        {
            if (!color)
            {
                return square.BlackPressure;
            }
            else
            {
                return square.WhitePressure;
            }
        }

        private static bool GetKingPressure(bool color, BoardSquare square)
        {
            if (!color)
            {
                return square.BlackKingPressure;
            }
            else
            {
                return square.WhiteKingPressure;
            }
        }
    }
}
