using ChessApi.Models.API;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChessApi.Models.DB
{
    public class Game
    {
        [BsonId]
        public ObjectId GameID { get; set; }
        public required Board Board { get; set; }
        public bool IsPlayerWhite { get; set; }
        public bool IsTwoPlayer { get; set; }
        public bool IsWhiteTurn { get; set; } = true;
        public string Status { get; set; } = "Open";
        public List<int[]> AvailableMoves { get; set; } = new();
        public int[]? SelectedSquare { get; set; } = null;
        public bool? CheckedColor { get; set; } = null;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public void SetStatus(GameStatus status)
        {
            this.Status = status.ToString();
        }
    }

    public enum GameStatus
    {
        Open,
        Expired,
        CheckMateWhite,
        CheckMateBlack,
        UserEnded,
    }
}
