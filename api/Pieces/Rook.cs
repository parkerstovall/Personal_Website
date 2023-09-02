﻿using api.helperclasses;
using api.models.api;
using api.pieces.interfaces;

namespace api.pieces
{
    public class Rook : IPiece, IPieceHasMoved
    {
        public string Color { get; set; }
        public bool HasMoved { get; set; } = false;

        public Rook(string Color)
        {
            this.Color = Color;
        }

        public List<int[]> GetPaths(Board board, int[] coords, bool check)
        {
            List<int[]> moves = new();

            int col = coords[0];
            int row = coords[1];
            int[] colInc = { 0, 0, 1, -1 };
            int[] rowInc = { 1, -1, 0, 0 };

            Direction[] dir =
            {
                Direction.FromLeftToRight,
                Direction.FromLeftToRight,
                Direction.FromTopToBottom,
                Direction.FromTopToBottom
            };

            for (int i = 0; i < 4; i++, col = coords[0], row = coords[1])
            {
                if (
                    board.Rows[col].Squares[row].PinnedDirection != Direction.None
                    && board.Rows[col].Squares[row].PinnedDirection != dir[i]
                )
                {
                    continue;
                }

                col += colInc[i];
                row += rowInc[i];

                while (PieceHelper.IsInBoard(col, row))
                {
                    if (board.Rows[col].Squares[row].Piece == null)
                    {
                        if (check)
                        {
                            if (board.Rows[col].Squares[row].CheckBlockingColor == this.Color)
                            {
                                moves.Add(new int[] { col, row });
                            }
                        }
                        else
                        {
                            moves.Add(new int[] { col, row });
                        }
                    }
                    else if (board.Rows[col].Squares[row].Piece?.Color != this.Color)
                    {
                        if (check)
                        {
                            if (board.Rows[col].Squares[row].CheckBlockingColor == this.Color)
                            {
                                moves.Add(new int[] { col, row });
                            }
                        }
                        else
                        {
                            moves.Add(new int[] { col, row });
                        }
                        break;
                    }
                    else
                    {
                        break;
                    }

                    col += colInc[i];
                    row += rowInc[i];
                }
            }

            return moves;
        }

        public List<int[]> GetPressure(Board board, int[] coords)
        {
            List<int[]> moves = new();

            int col = coords[0];
            int row = coords[1];
            int[] colInc = { 0, 0, 1, -1 };
            int[] rowInc = { 1, -1, 0, 0 };

            for (int i = 0; i < 4; i++, col = coords[0], row = coords[1])
            {
                col += colInc[i];
                row += rowInc[i];

                while (col >= 0 && row >= 0 && col < 8 && row < 8)
                {
                    moves.Add(new int[] { col, row });

                    if (board.Rows[col].Squares[row].Piece != null)
                    {
                        if (board.Rows[col].Squares[row].Piece is King)
                        {
                            col += colInc[i];
                            row += rowInc[i];
                            if (col >= 0 && row >= 0 && col < 8 && row < 8)
                            {
                                moves.Add(new int[] { col, row });
                            }
                        }

                        break;
                    }

                    col += colInc[i];
                    row += rowInc[i];
                }
            }

            return moves;
        }

        public string ToString(bool pipeSeparated)
        {
            if (pipeSeparated)
            {
                return Color + "|Rook";
            }

            return Color + "Rook";
        }
    }
}
