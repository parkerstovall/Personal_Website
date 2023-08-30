using api.models.client;
using api.models.api;
using Microsoft.Extensions.Caching.Memory;
using api.helperclasses;

namespace api.repository
{
    public class GameRepository
    {
        private readonly IMemoryCache _cache;

        public GameRepository(IMemoryCache cache)
        {
            _cache = cache;
        }

        public GameStart StartGame()
        {
            int gameID = -1;

            if (_cache.TryGetValue("OpenGames", out List<int>? openGames))
            {
                gameID = openGames == null ? 0 : openGames.Count;
            }
            else
            {
                openGames = new();
            }

            openGames?.Add(gameID);

            _cache.Set("OpenGames", openGames);

            Board board = BoardHelper.GetNewBoard();
            _cache.Set($"Board:{gameID}", board);
            return new GameStart
            {
                Board = BoardHelper.GetBoardForDisplay(board, null),
                GameID = gameID
            };
        }

        public BoardDisplay HandleClick(int gameID, int row, int col)
        {
            Board board = _cache.Get<Board>($"Board:{gameID}") ?? BoardHelper.GetNewBoard();
            List<int[]> moves = _cache.Get<List<int[]>>($"Moves:{gameID}") ?? new();
            int[] start = _cache.Get<int[]>($"SelectedSquare:{gameID}") ?? Array.Empty<int>();

            if (
                moves.Any()
                && start.Any()
                && MoveHelpers.TryMovePiece(row, col, start, ref moves, ref board)
            )
            {
                _cache.Remove($"Moves:{gameID}");
                _cache.Remove($"SelectedSquare:{gameID}");
            }
            else
            {
                moves = MoveHelpers.GetMovesFromPiece(board, row, col);
                _cache.Set($"Moves:{gameID}", moves);
                _cache.Set($"SelectedSquare:{gameID}", new[] { row, col });
            }

            return BoardHelper.GetBoardForDisplay(board, moves);
        }
    }
}
