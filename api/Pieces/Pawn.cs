using ChessApi.HelperClasses.Chess;
using ChessApi.Models.API;
using ChessApi.Pieces.Interfaces;

namespace ChessApi.Pieces
{
    public class Pawn(bool Color) : IPieceDirectAttacker, IPieceHasMoved
    {
        private static readonly string HashKeyBlack = "p0";
        private static readonly string HashKeyWhite = "p1";

        public bool HasMoved { get; set; } = false;
        public bool Color { get; set; } = Color;
        public Direction PinnedDir { get; set; } = Direction.None;

        public int[,] WhiteValues { get; } =
            new int[,]
            {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 50, 50, 50, 50, 50, 50, 50, 50 },
                { 10, 10, 20, 30, 30, 20, 10, 10 },
                { 5, 5, 10, 25, 25, 10, 5, 5 },
                { 0, 0, 0, 20, 20, 0, 0, 0 },
                { 5, -5, -10, 0, 0, -10, -5, 5 },
                { 5, 10, 10, -20, -20, 10, 10, 5 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
            };

        public int[,] BlackValues { get; } =
            new int[,]
            {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 5, 10, 10, -20, -20, 10, 10, 5 },
                { 5, -5, -10, 0, 0, -10, -5, 5 },
                { 0, 0, 0, 20, 20, 0, 0, 0 },
                { 5, 5, 10, 25, 25, 10, 5, 5 },
                { 10, 10, 20, 30, 30, 20, 10, 10 },
                { 50, 50, 50, 50, 50, 50, 50, 50 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
            };

        public int Value { get; } = 100;

        public List<PossibleMove> GetPaths(Board board, int[] coords, bool check)
        {
            List<PossibleMove> moves = [];
            int dir = Color ? 1 : -1;

            int col = coords[0] + dir;
            int row = coords[1];

            bool canMove = (
                PinnedDir == Direction.None
                || PinnedDir == Direction.FromTopToBottom
                || PinnedDir == Direction.FromBottomToTop
            );

            bool canAttackLeft = (
                PinnedDir == Direction.None
                || PinnedDir == Direction.FromBottomRightToTopLeft
                || PinnedDir == Direction.FromTopLeftToBottomRight
            );

            bool canAttackRight = (
                PinnedDir == Direction.None
                || PinnedDir == Direction.FromBottomLeftToTopRight
                || PinnedDir == Direction.FromTopRightToBottomLeft
            );

            if (col > -1 && col < board.Rows.Count)
            {
                //Moving
                if (board.Rows[col].Squares[row].Piece is null && canMove)
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

                    col += dir;

                    if (
                        col > -1
                        && col < board.Rows.Count
                        && !HasMoved
                        && board.Rows[col].Squares[row].Piece is null
                    )
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
                }

                col = coords[0] + dir;
                row -= 1;

                //Attacking
                if (row > -1)
                {
                    if (canAttackLeft)
                    {
                        BoardSquare left = board.Rows[col].Squares[row];

                        if (
                            (left.Piece is not null && left.Piece?.Color != this.Color)
                            || (left.EnPassantColor.HasValue && left.EnPassantColor != this.Color)
                        )
                        {
                            if (!check || left.CheckBlockingColor == this.Color)
                            {
                                var capturedPiece = left.Piece;
                                int[]? capturedFrom;
                                if (left.EnPassantColor.HasValue)
                                {
                                    capturedFrom = [left.Coords[0] - dir, left.Coords[1]];
                                    capturedPiece = board.Rows[col - dir].Squares[row].Piece;
                                }
                                else
                                {
                                    capturedFrom = left.Coords;
                                }

                                moves.Add(
                                    new()
                                    {
                                        MoveTo = [col, row],
                                        MoveFrom = [coords[0], coords[1]],
                                        MovingPiece = this,
                                        CapturedPiece = capturedPiece,
                                        CapturedMoveFromOverride = capturedFrom,
                                    }
                                );
                            }
                        }
                    }
                }

                row += 2;

                if (row < board.Rows[col].Squares.Count)
                {
                    if (canAttackRight)
                    {
                        BoardSquare right = board.Rows[col].Squares[row];
                        if (
                            (right.Piece is not null && right.Piece?.Color != this.Color)
                            || (right.EnPassantColor.HasValue && right.EnPassantColor != this.Color)
                        )
                        {
                            if (!check || right.CheckBlockingColor == this.Color)
                            {
                                var capturedPiece = right.Piece;
                                int[]? capturedFrom;
                                if (right.EnPassantColor.HasValue)
                                {
                                    capturedFrom = [right.Coords[0] - dir, right.Coords[1]];
                                    capturedPiece = board.Rows[col - dir].Squares[row].Piece;
                                }
                                else
                                {
                                    capturedFrom = right.Coords;
                                }

                                moves.Add(
                                    new()
                                    {
                                        MoveTo = [col, row],
                                        MoveFrom = [coords[0], coords[1]],
                                        MovingPiece = this,
                                        CapturedPiece = capturedPiece,
                                        CapturedMoveFromOverride = capturedFrom,
                                    }
                                );
                            }
                        }
                    }
                }
            }

            return moves;
        }

        public List<int[]> GetPressure(Board board, int[] coords)
        {
            List<int[]> moves = [];
            int dir = Color ? 1 : -1;

            int i = coords[0] + dir;
            int j = coords[1] - 1;

            if (i > -1 && i < board.Rows.Count)
            {
                //Attacking
                if (j > -1)
                {
                    moves.Add(board.Rows[i].Squares[j].Coords);
                }

                j += 2;

                if (j < board.Rows[i].Squares.Count)
                {
                    moves.Add(board.Rows[i].Squares[j].Coords);
                }
            }

            return moves;
        }

        public IPiece Copy()
        {
            Pawn newPiece = new(this.Color)
            {
                HasMoved = this.HasMoved,
                PinnedDir = this.PinnedDir,
            };
            return newPiece;
        }

        public string GetHashKey()
        {
            return Color ? HashKeyBlack : HashKeyWhite;
        }

        public override string ToString()
        {
            return (Color == false ? "white" : "black") + "Pawn";
        }
    }
}
