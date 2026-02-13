using ChessApi.HelperClasses.Chess;
using ChessApi.Models.API;
using ChessApi.Pieces.Interfaces;

namespace ChessApi.Pieces
{
    public class King(bool Color) : IPieceHasMoved
    {
        private static readonly int[] KingColInc = { 0, 0, 1, -1, 1, -1, 1, -1 };
        private static readonly int[] KingRowInc = { 1, -1, 0, 0, 1, -1, -1, 1 };
        private static readonly string HashKeyBlack = "k0";
        private static readonly string HashKeyWhite = "k1";

        public bool Color { get; set; } = Color;
        public bool HasMoved { get; set; } = false;
        public bool InCheck { get; set; } = false;
        public bool InCheckMate { get; set; } = false;
        public Direction PinnedDir { get; set; } = Direction.None;

        public int[,] WhiteValues { get; } =
            new int[,]
            {
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -20, -30, -30, -40, -40, -30, -30, -20 },
                { -10, -20, -20, -20, -20, -20, -20, -10 },
                { 20, 20, 0, 0, 0, 0, 20, 20 },
                { 20, 30, 10, 0, 0, 10, 30, 20 },
            };

        public int[,] BlackValues { get; } =
            new int[,]
            {
                { 20, 30, 10, 0, 0, 10, 30, 20 },
                { 20, 20, 0, 0, 0, 0, 20, 20 },
                { -10, -20, -20, -20, -20, -20, -20, -10 },
                { -20, -30, -30, -40, -40, -30, -30, -20 },
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -30, -40, -40, -50, -50, -40, -40, -30 },
            };
        public int Value { get; } = 20000;

        public List<PossibleMove> GetPaths(Board board, int[] coords, bool check)
        {
            List<PossibleMove> moves = [];

            int col = coords[0];
            int row = coords[1];

            for (int i = 0; i < 8; i++, col = coords[0], row = coords[1])
            {
                col += KingColInc[i];
                row += KingRowInc[i];

                if (PieceHelper.IsInBoard(col, row))
                {
                    BoardSquare square = board.Rows[col].Squares[row];
                    if (
                        (square.Piece is null || square.Piece?.Color != this.Color)
                        && SafeSquare(square)
                    )
                    {
                        moves.Add(
                            new()
                            {
                                MoveTo = [col, row],
                                MoveFrom = [coords[0], coords[1]],
                                MovingPiece = this,
                                CapturedPiece = square.Piece,
                            }
                        );
                    }
                }
            }

            CheckCastle(board, coords, ref moves);

            return moves;
        }

        public List<int[]> GetPressure(Board board, int[] coords)
        {
            List<int[]> moves = new();
            int col = coords[0];
            int row = coords[1];

            for (int i = 0; i < 8; i++, col = coords[0], row = coords[1])
            {
                col += KingColInc[i];
                row += KingRowInc[i];

                if (PieceHelper.IsInBoard(col, row))
                {
                    moves.Add(board.Rows[col].Squares[row].Coords);
                }
            }

            return moves;
        }

        public override string ToString()
        {
            return (Color == false ? "white" : "black") + "King";
        }

        private void CheckCastle(Board board, int[] coords, ref List<PossibleMove> moves)
        {
            if (HasMoved || InCheck)
            {
                return;
            }

            int col = coords[0];
            int row = coords[1];

            if (CheckCastleDir(board, true, col, row))
            {
                //moves.Add(new int[] { col, row - 2 });
                moves.Add(
                    new()
                    {
                        MoveTo = [col, row - 2],
                        MoveFrom = [coords[0], coords[1]],
                        MovingPiece = this,
                        CapturedPiece = board.Rows[col].Squares[row - 4].Piece,
                        CapturedMoveFromOverride = [col, row - 4],
                        CapturedMoveToOverride = [col, row - 1],
                    }
                );
            }

            if (CheckCastleDir(board, false, col, row))
            {
                //moves.Add(new int[] { col, row + 2 });
                moves.Add(
                    new()
                    {
                        MoveTo = [col, row + 2],
                        MoveFrom = [coords[0], coords[1]],
                        MovingPiece = this,
                        CapturedPiece = board.Rows[col].Squares[row + 3].Piece,
                        CapturedMoveFromOverride = [col, row + 3],
                        CapturedMoveToOverride = [col, row + 1],
                    }
                );
            }
        }

        private bool CheckCastleDir(Board board, bool left, int col, int row)
        {
            int offset = left ? -1 : 1;
            int len = left ? 4 : 3;
            bool add = true;
            for (int i = 1; i < len; i++)
            {
                BoardSquare square = board.Rows[col].Squares[row + (i * offset)];

                if (i + 1 == len)
                {
                    if (square.Piece is IPieceHasMoved hm && hm.HasMoved)
                    {
                        add = false;
                        break;
                    }
                }
                if (square.Piece is not null || !SafeSquare(square))
                {
                    add = false;
                    break;
                }
            }

            return add;
        }

        private bool SafeSquare(BoardSquare square)
        {
            if (!Color)
            {
                return square.BlackPressure == 0;
            }
            else
            {
                return square.WhitePressure == 0;
            }
        }

        public string GetHashKey()
        {
            return Color ? HashKeyBlack : HashKeyWhite;
        }

        public IPiece Copy()
        {
            King newPiece = new(this.Color)
            {
                PinnedDir = this.PinnedDir,
                InCheck = this.InCheck,
                InCheckMate = this.InCheckMate,
                HasMoved = this.HasMoved,
            };
            return newPiece;
        }
    }
}
