using api.models.client;
using ChessApi.HelperClasses.Chess;
using ChessApi.Models.API;
using ChessApi.Models.DB;
using ChessApi.Pieces.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api.repository
{
    public class GameRepository(ConnectionRepository connRepo, IHttpContextAccessor context)
    {
        private readonly ConnectionRepository _connRepo = connRepo;
        private readonly IHttpContextAccessor _context = context;

        public async Task<SavedGameResult> TryGetSavedGame()
        {
            var gameID = _context?.HttpContext?.Request.Cookies["GameID"];

            if (gameID is not null && ObjectId.TryParse(gameID, out ObjectId oGameID))
            {
                var game = await GetsertGame();
                return new()
                {
                    BoardDisplay = BoardHelper.GetBoardForDisplay(game),
                    IsPlayerWhite = game.IsPlayerWhite,
                    IsTwoPlayer = game.IsTwoPlayer,
                };
            }

            return new();
        }

        public async Task<BoardDisplay> StartGame(bool isWhite, bool isTwoPlayer)
        {
            //Create new Game
            var game = new Game()
            {
                Board = BoardHelper.GetNewBoard(),
                IsPlayerWhite = isWhite,
                IsTwoPlayer = isTwoPlayer,
            };

            // Add cookie to response, save game to DB
            await GetsertGame(game, true);

            return BoardHelper.GetBoardForDisplay(game);
        }

        public async Task<ClickReturn> HandleClick(int row, int col)
        {
            Game game = await GetsertGame();

            bool moved = false;
            int[]? clickedSquare = [row, col];
            BoardSquare square = game.Board.Rows[row].Squares[col];

            if (
                game.AvailableMoves.Count != 0
                && game.SelectedSquare is not null
                && MoveHelper.TryMovePiece(
                    clickedSquare,
                    game.SelectedSquare,
                    game.AvailableMoves,
                    ref game
                )
            )
            {
                IPiece? piece = game.Board.Rows[row].Squares[col].Piece;
                if (piece is not null && game.SelectedSquare is not null)
                {
                    var moveHistory = _connRepo.GetCollection<Move>("MoveHistory");
                    await moveHistory.InsertOneAsync(
                        new Move
                        {
                            GameID = game.GameID,
                            From = game.SelectedSquare,
                            To = clickedSquare,
                            PieceColor = piece.Color,
                            PieceType = piece.GetType().Name,
                        }
                    );
                }

                game.AvailableMoves.Clear();
                game.IsWhiteTurn = !game.IsWhiteTurn;
                game.SelectedSquare = null;
                moved = true;
            }
            else if (square.Piece is null || square.Piece.Color == game.IsWhiteTurn)
            {
                game.AvailableMoves.Clear();
                game.SelectedSquare = null;
                return new() { Moved = false, Board = BoardHelper.GetBoardForDisplay(game) };
            }
            else
            {
                game.AvailableMoves =
                [
                    .. MoveHelper
                        .GetMovesFromPiece(game.Board, clickedSquare, game.CheckedColor)
                        .Select(p => p.MoveTo),
                ];
                game.SelectedSquare = clickedSquare;
            }

            game = await GetsertGame(game);

            return new() { Moved = moved, Board = BoardHelper.GetBoardForDisplay(game) };
        }

        public async Task<BoardDisplay> CompMove()
        {
            var game = await GetsertGame();
            if (game.IsWhiteTurn == game.IsPlayerWhite)
            {
                return BoardHelper.GetBoardForDisplay(game);
            }

            var options = new MinMaxEngineOptions
            {
                MaxDepth = 3,
                MaxTime = 10000,
                BoardSize = 8,
                PieceHashKeys =
                [
                    "r0",
                    "n0",
                    "b0",
                    "q0",
                    "k0",
                    "p0",
                    "r1",
                    "n1",
                    "b1",
                    "q1",
                    "k1",
                    "p1",
                ],
            };
            MinMaxEngine ai = new(options);
            var foundMove = ai.GetMove(game);
            //var foundMove = ai.GetMoveParallel(game);
            if (foundMove is not null)
            {
                await _connRepo.GetCollection<Move>("MoveHistory").InsertOneAsync(foundMove);

                MoveHelper.MovePiece(
                    [foundMove.From[0], foundMove.From[1]],
                    [foundMove.To[0], foundMove.To[1]],
                    ref game
                );
            }

            game.IsWhiteTurn = !game.IsWhiteTurn;
            game = await GetsertGame(game);

            return BoardHelper.GetBoardForDisplay(game);
        }

        private async Task<Game> GetsertGame(Game? updateGame = null, bool forceInsert = false)
        {
            var collection = _connRepo.GetCollection<Game>("Games");
            var gameID = _context?.HttpContext?.Request.Cookies["GameID"];
            if (gameID is not null && ObjectId.TryParse(gameID, out ObjectId oGameID))
            {
                var filter = Builders<Game>.Filter.Eq("_id", oGameID);
                if (forceInsert)
                {
                    //End the current game, then start a new one after the if block
                    var update = Builders<Game>.Update.Set(
                        game => game.Status,
                        GameStatus.UserEnded.ToString()
                    );

                    await collection.UpdateOneAsync(filter, update);
                }
                else
                {
                    //Update and return if game is provided
                    if (updateGame is not null)
                    {
                        updateGame.GameID = oGameID;
                        var replaceOptions = new ReplaceOptions() { IsUpsert = true };
                        await collection.ReplaceOneAsync(filter, updateGame, replaceOptions);
                        return updateGame;
                    }

                    // Get if no game is provided
                    var dbGame = await collection.Find(filter).FirstOrDefaultAsync();
                    if (dbGame is not null)
                    {
                        return dbGame;
                    }
                }
            }

            //Insert if no game ID in cookie and game is provided
            if (updateGame is not null)
            {
                await collection.InsertOneAsync(updateGame);
                _context?.HttpContext?.Response.Cookies.Append(
                    "GameID",
                    updateGame.GameID.ToString()
                );
                return updateGame;
            }

            // Otherwise, bad request
            throw new InvalidOperationException("GameID or UpdateGame must be present");
        }
    }
}
