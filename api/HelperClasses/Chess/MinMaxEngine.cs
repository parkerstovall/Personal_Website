using System.Collections.Concurrent;
using ChessApi.Models.API;
using ChessApi.Models.DB;
using ChessApi.Pieces;
using ChessApi.Pieces.Interfaces;

namespace ChessApi.HelperClasses.Chess
{
    /// <summary>
    ///  Options for the MinMaxEngine, including max depth, max time, player colors, and board size.
    ///  These options can be adjusted to optimize the performance of the MinMax algorithm.
    /// </summary>
    ///
    public class MinMaxEngineOptions
    {
        // /// <summary>
        // ///  Whether to use parallel processing for the MinMax algorithm.
        // ///  Setting this to true can speed up the search process on multi-core systems.
        // ///  However, it may also increase memory usage and complexity.
        // /// </summary>
        // public bool UseParallel { get; set; } = false;

        // /// <summary>
        // ///  Whether to use transpositions to store previously evaluated positions.
        // ///  This can significantly reduce computation time by avoiding redundant calculations.
        // ///  However, it may increase memory usage.
        // /// </summary>
        // public bool UseTranspositions { get; set; } = true;

        /// <summary>
        ///  The maximum depth for the MinMax algorithm. This determines how many moves ahead the engine will evaluate.
        ///  A higher value will result in a more thorough search but will also increase computation time.
        ///  If it is set to -1, the engine will increase the max depth based on the number of possible moves.
        /// </summary>
        public int MaxDepth { get; set; } = 10;

        /// <summary>
        ///  The maximum time (in milliseconds) the engine will spend searching for a move.
        ///  If the time limit is reached, the engine will return the best move found so far.
        /// </summary>
        public int? MaxTime { get; set; } = 10000;

        /// <summary>
        ///  Colors are represented as false (black) and true (black).
        ///  </summart>
        public bool MaxPlayer { get; set; } = true;

        /// <summary>
        ///  The size of the board.
        ///  </summary>
        public required byte BoardSize { get; set; }

        /// <summary>
        /// Hash keys for the pieces on the board.
        /// These should correspond with the piece types and the IPiece interface's GetHashKey method.
        /// There should be one value for each piece type and color combination.
        ///  </summary>
        public required string[] PieceHashKeys { get; set; }
    }

    public class MinMaxEngine
    {
        private int Max_Depth;
        private readonly bool Max_Player;
        private readonly long[,,] PositionHashes;
        private bool StopThinking = false;
        private readonly System.Timers.Timer timer = new();
        private readonly ConcurrentDictionary<long, int> Transpositions = new();
        private readonly Dictionary<string, byte> PieceHashKeys = new(
            StringComparer.OrdinalIgnoreCase
        );

        public MinMaxEngine(MinMaxEngineOptions options)
        {
            Max_Depth = options.MaxDepth;
            Max_Player = options.MaxPlayer;

            for (var i = 0; i < options.PieceHashKeys.Length; i++)
            {
                PieceHashKeys.Add(options.PieceHashKeys[i], (byte)i);
            }

            PositionHashes = new long[
                options.BoardSize,
                options.BoardSize,
                options.PieceHashKeys.Length
            ];

            GeneratePositionHashes();

            if (options.MaxTime.HasValue)
            {
                SetTimer(options.MaxTime.Value);
            }
        }

        private void SetTimer(int maxTime)
        {
            timer.Interval = maxTime;
            timer.Enabled = true;
            timer.AutoReset = false;
            timer.Elapsed += (o, e) => StopThinking = true;
        }

        public Move GetMove(Game game)
        {
            timer.Start();
            var possibleMoves = new List<PossibleMove>();

            foreach (BoardRow row in game.Board.Rows)
            {
                foreach (BoardSquare square in row.Squares)
                {
                    if (square.Piece is not null && square.Piece?.Color == Max_Player)
                    {
                        var moves = MoveHelper.GetMovesFromPiece(
                            game.Board,
                            square.Coords,
                            game.CheckedColor
                        );

                        possibleMoves.AddRange(moves);
                    }
                }
            }

            if (possibleMoves.Count < 10)
            {
                Max_Depth = 5;
            }
            else if (possibleMoves.Count < 5)
            {
                Max_Depth = 6;
            }
            else
            {
                Max_Depth = 4;
            }

            OrderPossibleMoves(possibleMoves);

            int alpha = int.MinValue;
            PossibleMove? move = null;
            var boardHash = GenerateBoardHash(game.Board);

            foreach (var pMove in possibleMoves)
            {
                var newGame = CopyGame(game);
                var tempBoardHash = MovePiece(boardHash, pMove, ref newGame);
                int tempScore = MinMax(newGame, false, 0, alpha, int.MaxValue, tempBoardHash);

                if (tempScore > alpha)
                {
                    alpha = tempScore;
                    move = pMove;
                }
            }

            if (move is null)
            {
                throw new Exception("No suitable moves found.");
            }

            Move foundMove = new()
            {
                GameID = game.GameID,
                From = [move.MoveFrom[0], move.MoveFrom[1]],
                To = [move.MoveTo[0], move.MoveTo[1]],
                PieceColor = move.MovingPiece.Color,
                PieceType = move.MovingPiece.GetType().Name,
            };

            return foundMove;
        }

        public Move GetMoveParallel(Game game)
        {
            timer.Start();
            var possibleMoves = new List<PossibleMove>();
            var threadResources = new ThreadResources();

            foreach (BoardRow row in game.Board.Rows)
            {
                foreach (BoardSquare square in row.Squares)
                {
                    if (square.Piece is not null && square.Piece?.Color == Max_Player)
                    {
                        var moves = MoveHelper.GetMovesFromPiece(
                            game.Board,
                            square.Coords,
                            game.CheckedColor
                        );

                        possibleMoves.AddRange(moves);
                    }
                }
            }

            if (possibleMoves.Count < 10)
            {
                Max_Depth = 6;
            }
            else if (possibleMoves.Count < 5)
            {
                Max_Depth = 7;
            }
            else
            {
                Max_Depth = 5;
            }

            OrderPossibleMoves(possibleMoves);

            // https://www.chessprogramming.org/Young_Brothers_Wait_Concept
            // We run the first move to get a baseline score and hopefully
            // speed up the rest of the moves when they are run in parralel
            // (Alpha Beta Pruning uses the initial score to compare moves)
            var boardHash = GenerateBoardHash(game.Board);
            var firstMove = possibleMoves[0];
            threadResources.Move = firstMove;
            possibleMoves.RemoveAt(0);

            Game firstGame = CopyGame(game);
            var firstTempBoardHash = MovePiece(boardHash, firstMove, ref firstGame);

            threadResources.Alpha = MinMax(
                firstGame,
                false,
                0,
                int.MinValue,
                int.MaxValue,
                firstTempBoardHash
            );

            Parallel.ForEach(
                possibleMoves,
                pMove =>
                {
                    var newGame = CopyGame(game);
                    var tempBoardHash = MovePiece(boardHash, pMove, ref newGame);

                    int tempScore = MinMax(
                        newGame,
                        false,
                        0,
                        threadResources.Alpha,
                        int.MaxValue,
                        tempBoardHash
                    );
                    if (tempScore > threadResources.Alpha)
                    {
                        lock (threadResources)
                        {
                            threadResources.Alpha = tempScore;
                            threadResources.Move = pMove;
                        }
                    }
                }
            );

            Move foundMove = new()
            {
                GameID = game.GameID,
                From = [threadResources.Move.MoveFrom[0], threadResources.Move.MoveFrom[1]],
                To = [threadResources.Move.MoveTo[0], threadResources.Move.MoveTo[1]],
                PieceColor = threadResources.Move.MovingPiece.Color,
                PieceType = threadResources.Move.MovingPiece.GetType().Name,
            };

            return foundMove;
        }

        private int MinMax(Game game, bool isMax, int depth, int alpha, int beta, long boardHash)
        {
            var color = isMax ? Max_Player : !Max_Player;
            if (Transpositions.TryGetValue(boardHash, out int boardScore))
            {
                return boardScore;
            }

            if (depth == Max_Depth || StopThinking)
            {
                boardScore = GetBoardScore(game.Board);
                Transpositions.TryAdd(boardHash, boardScore);
                return boardScore;
            }

            List<BoardSquare> moveSquares = new();

            foreach (BoardRow row in game.Board.Rows)
            {
                foreach (BoardSquare square in row.Squares)
                {
                    if (square.Piece is not null && square.Piece?.Color == color)
                    {
                        if (square.Piece is King king && king.InCheckMate)
                        {
                            return isMax ? int.MinValue : int.MaxValue;
                        }

                        moveSquares.Add(square);
                    }
                }
            }

            List<PossibleMove> possibleMoves = [];
            foreach (BoardSquare square in moveSquares)
            {
                if (square.Piece is null)
                {
                    continue;
                }

                List<PossibleMove> localMoves = MoveHelper.GetMovesFromPiece(
                    game.Board,
                    square.Coords,
                    game.CheckedColor
                );

                possibleMoves.AddRange(localMoves);
            }

            if (possibleMoves.Count == 0)
            {
                return 0;
            }

            OrderPossibleMoves(possibleMoves);

            int score = isMax ? int.MinValue : int.MaxValue;

            foreach (var pMove in possibleMoves)
            {
                Game newGame = CopyGame(game);
                var tempBoardHash = MovePiece(boardHash, pMove, ref newGame);
                int tempScore = MinMax(newGame, !isMax, depth + 1, alpha, beta, tempBoardHash);

                if ((tempScore > score && isMax) || (tempScore < score && !isMax))
                {
                    score = tempScore;
                }

                if (isMax)
                {
                    alpha = Math.Max(score, alpha);
                    if (alpha >= beta)
                    {
                        break;
                    }
                }
                else
                {
                    beta = Math.Min(score, beta);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
            }

            Transpositions.TryAdd(boardHash, score);

            return score;
        }

        public int GetBoardScore(Board board)
        {
            int boardScore = 0;

            foreach (BoardRow row in board.Rows)
            {
                foreach (BoardSquare square in row.Squares)
                {
                    IPiece? piece = square.Piece;
                    if (piece is not null)
                    {
                        int[,] modifier =
                            piece.Color == Max_Player ? piece.BlackValues : piece.WhiteValues;
                        int pieceValue = piece.Value + modifier[square.Coords[0], square.Coords[1]];

                        if (piece.Color == Max_Player)
                        {
                            boardScore += pieceValue;
                        }
                        else
                        {
                            boardScore -= pieceValue;
                        }
                    }
                }
            }

            return boardScore;
        }

        public Game CopyGame(Game game)
        {
            Game newGame = new() { Board = new(), CheckedColor = game.CheckedColor };

            int i = 0;
            foreach (BoardRow row in game.Board.Rows)
            {
                newGame.Board.Rows.Add(new());
                foreach (BoardSquare square in row.Squares)
                {
                    newGame.Board.Rows[i].Squares.Add(square.Copy());
                }
                i++;
            }

            return newGame;
        }

        public void OrderPossibleMoves(List<PossibleMove> possibleMoves)
        {
            possibleMoves.Sort(
                (a, b) =>
                {
                    // Captures first, sorted descending by captured piece value
                    bool aCapture = a.CapturedPiece is not null;
                    bool bCapture = b.CapturedPiece is not null;

                    if (aCapture && bCapture)
                    {
                        return (b.CapturedPiece?.Value ?? 0).CompareTo(a.CapturedPiece?.Value ?? 0);
                    }

                    if (aCapture != bCapture)
                    {
                        return aCapture ? -1 : 1;
                    }

                    // Non-captures: prioritize pieces that have already moved
                    int aVal = a.HasMoved ? a.MovingPiece.Value + 1 : a.MovingPiece.Value;
                    int bVal = b.HasMoved ? b.MovingPiece.Value + 1 : b.MovingPiece.Value;
                    return aVal.CompareTo(bVal);
                }
            );
        }

        private void GeneratePositionHashes()
        {
            Random rand = new();
            HashSet<long> usedHashes = new();
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    foreach (var item in PieceHashKeys)
                    {
                        long value;

                        do
                        {
                            value = rand.NextInt64(0, long.MaxValue);
                        } while (!usedHashes.Add(value));

                        PositionHashes[i, j, item.Value] = value;
                    }
                }
            }
        }

        private long GenerateBoardHash(Board board)
        {
            long boardHash = 0;
            for (var i = 0; i < board.Rows.Count; i++)
            {
                var squareLen = board.Rows[i].Squares.Count;
                for (var j = 0; j < squareLen; j++)
                {
                    boardHash = AlterBoardHash(i, j, board, boardHash);
                }
            }

            return boardHash;
        }

        private long AlterBoardHash(int col, int row, Board board, long boardHash)
        {
            var piece = board.Rows[col].Squares[row].Piece;
            if (piece is null)
            {
                return boardHash;
            }

            var key = piece.GetHashKey();
            var pieceHash = PieceHashKeys[key];
            return boardHash ^= PositionHashes[col, row, pieceHash];
        }

        private long AlterBoardHash(int col, int row, IPiece piece, long boardHash)
        {
            var key = piece.GetHashKey();
            var pieceHash = PieceHashKeys[key];
            return boardHash ^= PositionHashes[col, row, pieceHash];
        }

        private long MovePiece(long boardHash, PossibleMove pMove, ref Game newGame)
        {
            // Remove the piece from its position in the hash
            long tempBoardHash = AlterBoardHash(
                pMove.MoveFrom[0],
                pMove.MoveFrom[1],
                pMove.MovingPiece,
                boardHash
            );

            // Move the piece on the actual board
            MoveHelper.MovePiece(
                [pMove.MoveFrom[0], pMove.MoveFrom[1]],
                [pMove.MoveTo[0], pMove.MoveTo[1]],
                ref newGame
            );

            // Add the piece to its new position in the hash
            tempBoardHash = AlterBoardHash(
                pMove.MoveTo[0],
                pMove.MoveTo[1],
                pMove.MovingPiece,
                tempBoardHash
            );

            // Account for pawn promotions
            if (pMove.MovingPiece is Pawn p)
            {
                if (pMove.MoveTo[0] == 0 && !p.Color)
                {
                    var pieceHash = PieceHashKeys["q0"];
                    tempBoardHash ^= PositionHashes[pMove.MoveTo[0], pMove.MoveTo[1], pieceHash];
                }
                else if (pMove.MoveTo[0] == 7 && p.Color)
                {
                    var pieceHash = PieceHashKeys["q1"];
                    tempBoardHash ^= PositionHashes[pMove.MoveTo[0], pMove.MoveTo[1], pieceHash];
                }
            }

            // Remove the captured piece from the hash, if it exists.
            if (pMove.CapturedPiece is not null)
            {
                // Override in case of castling or en passant
                if (
                    pMove.CapturedMoveFromOverride is not null
                    || pMove.CapturedMoveToOverride is not null
                )
                {
                    if (pMove.CapturedMoveFromOverride is not null)
                    {
                        tempBoardHash = AlterBoardHash(
                            pMove.CapturedMoveFromOverride[0],
                            pMove.CapturedMoveFromOverride[1],
                            pMove.CapturedPiece,
                            tempBoardHash
                        );
                    }

                    if (pMove.CapturedMoveToOverride is not null)
                    {
                        tempBoardHash = AlterBoardHash(
                            pMove.CapturedMoveToOverride[0],
                            pMove.CapturedMoveToOverride[1],
                            pMove.CapturedPiece,
                            tempBoardHash
                        );
                    }
                }
                else
                {
                    // Normal capture
                    tempBoardHash = AlterBoardHash(
                        pMove.MoveTo[0],
                        pMove.MoveTo[1],
                        pMove.CapturedPiece,
                        tempBoardHash
                    );
                }
            }

            return tempBoardHash;
        }
    }
}
