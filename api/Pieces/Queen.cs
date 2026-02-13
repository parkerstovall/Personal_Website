using ChessApi.HelperClasses.Chess;
using ChessApi.Models.API;
using ChessApi.Models.DB;
using ChessApi.Pieces.Interfaces;

namespace ChessApi.Pieces
{
    public class Queen(bool Color) : IPieceCanPin
    {
        private static readonly int[] PressureColInc = { 0, 0, 1, -1, -1, -1, 1, 1 };
        private static readonly int[] PressureRowInc = { 1, -1, 0, 0, 1, -1, -1, 1 };
        private static readonly string HashKeyBlack = "q0";
        private static readonly string HashKeyWhite = "q1";

        public bool Color { get; set; } = Color;
        public Direction PinnedDir { get; set; } = Direction.None;
        public int[,] WhiteValues { get; } =
            new int[,]
            {
                { -20, -10, -10, -5, -5, -10, -10, -20 },
                { -10, 0, 0, 0, 0, 0, 0, -10 },
                { -10, 0, 5, 5, 5, 5, 0, -10 },
                { -5, 0, 5, 5, 5, 5, 0, -5 },
                { 0, 0, 5, 5, 5, 5, 0, -5 },
                { -10, 5, 5, 5, 5, 5, 0, -10 },
                { -10, 0, 5, 0, 0, 0, 0, -10 },
                { -20, -10, -10, -5, -5, -10, -10, -20 },
            };

        public int[,] BlackValues { get; } =
            new int[,]
            {
                { -20, -10, -10, -5, -5, -10, -10, -20 },
                { -10, 0, 5, 0, 0, 0, 0, -10 },
                { -10, 5, 5, 5, 5, 5, 0, -10 },
                { 0, 0, 5, 5, 5, 5, 0, -5 },
                { -5, 0, 5, 5, 5, 5, 0, -5 },
                { -10, 0, 5, 5, 5, 5, 0, -10 },
                { -10, 0, 0, 0, 0, 0, 0, -10 },
                { -20, -10, -10, -5, -5, -10, -10, -20 },
            };
        public int Value { get; } = 900;

        public List<PossibleMove> GetPaths(Board board, int[] coords, bool check)
        {
            List<PossibleMove> moves = [];

            int col = coords[0];
            int row = coords[1];

            Tuple<int[], int[]> increments = PieceHelper.GetIncrements(
                PinnedDir,
                diag: true,
                straight: true
            );
            int[] colInc = increments.Item1;
            int[] rowInc = increments.Item2;

            for (int i = 0; i < colInc.Length; i++, col = coords[0], row = coords[1])
            {
                col += colInc[i];
                row += rowInc[i];

                while (PieceHelper.IsInBoard(col, row))
                {
                    if (board.Rows[col].Squares[row].Piece is null)
                    {
                        if (!check || board.Rows[col].Squares[row].CheckBlockingColor == this.Color)
                        {
                            //moves.Add(new int[] { col, row });
                            moves.Add(
                                new()
                                {
                                    MoveTo = [col, row],
                                    MoveFrom = [coords[0], coords[1]],
                                    MovingPiece = this,
                                }
                            );
                        }
                    }
                    else if (board.Rows[col].Squares[row].Piece?.Color != this.Color)
                    {
                        if (!check || board.Rows[col].Squares[row].CheckBlockingColor == this.Color)
                        {
                            //moves.Add(new int[] { col, row });
                            moves.Add(
                                new()
                                {
                                    MoveTo = [col, row],
                                    MoveFrom = [coords[0], coords[1]],
                                    CapturedPiece = board.Rows[col].Squares[row].Piece,
                                    MovingPiece = this,
                                }
                            );
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

            for (int i = 0; i < 8; i++, col = coords[0], row = coords[1])
            {
                col += PressureColInc[i];
                row += PressureRowInc[i];

                while (PieceHelper.IsInBoard(col, row))
                {
                    moves.Add(board.Rows[col].Squares[row].Coords);

                    if (board.Rows[col].Squares[row].Piece is not null)
                    {
                        if (board.Rows[col].Squares[row].Piece is King)
                        {
                            col += PressureColInc[i];
                            row += PressureRowInc[i];
                            if (PieceHelper.IsInBoard(col, row))
                            {
                                moves.Add(board.Rows[col].Squares[row].Coords);
                            }
                        }
                        break;
                    }

                    col += PressureColInc[i];
                    row += PressureRowInc[i];
                }
            }

            return moves;
        }

        public bool CanPin(Board board, int[] start, int[] dest)
        {
            Direction dir = PieceHelper.GetDirection(start, dest);

            return GoodDir(dir);
        }

        public bool GoodDir(Direction dir)
        {
            return dir != Direction.None;
        }

        public void CheckPins(int[] start, int[] dest, ref Game game)
        {
            Direction dir = PieceHelper.GetDirection(start, dest);

            if (!GoodDir(dir))
            {
                return;
            }

            int[] inc = PieceHelper.GetSingleIncrement(dir);

            PieceHelper.SetPins(start, inc, dir, this.Color, ref game);
        }

        public bool HasSavingSquares(int[] start, int[] dest, ref Game game)
        {
            Direction dir = PieceHelper.GetDirection(start, dest);

            if (!GoodDir(dir))
            {
                return false;
            }

            int[] inc = PieceHelper.GetSingleIncrement(dir);

            return PieceHelper.SetSavingSquares(start, inc, this.Color, ref game);
        }

        public IPiece Copy()
        {
            Queen newPiece = new(this.Color) { PinnedDir = this.PinnedDir };
            return newPiece;
        }

        public string GetHashKey()
        {
            return Color ? HashKeyBlack : HashKeyWhite;
        }

        public override string ToString()
        {
            return (Color == false ? "white" : "black") + "Queen";
        }
    }
}
