using ChessApi.Models.API;
using ChessApi.Pieces;
using ChessApi.Pieces.Interfaces;

namespace ChessApi.HelperClasses.Chess
{
    public class CheckTracker
    {
        public BoardSquare? WhiteKing { get; set; } = null;
        public BoardSquare? BlackKing { get; set; } = null;
        public List<BoardSquare> PinPieces { get; set; } = [];
        public bool HasWhiteSavingSquares { get; set; } = false;
        public bool HasBlackSavingSquares { get; set; } = false;
        public List<BoardSquare> Attackers { get; set; } = [];

        public void SetKing(BoardSquare square)
        {
            if (square.Piece is not null && square.Piece is King king)
            {
                if (king.Color)
                {
                    BlackKing = square;
                }
                else
                {
                    WhiteKing = square;
                }
            }
        }

        public BoardSquare? GetKing(bool color)
        {
            return !color ? WhiteKing : BlackKing;
        }

        public List<BoardSquare> GetKingAttackers(bool color)
        {
            var result = new List<BoardSquare>(Attackers.Count);
            foreach (var a in Attackers)
            {
                if (a.Piece?.Color != color)
                {
                    result.Add(a);
                }
            }
            return result;
        }

        public void AddAttacker(BoardSquare attacker)
        {
            Attackers.Add(attacker);
        }

        public void SetHasSavingSquares(bool color, bool hasSavingSquares)
        {
            if (!color)
            {
                HasWhiteSavingSquares = hasSavingSquares;
            }
            else
            {
                HasBlackSavingSquares = hasSavingSquares;
            }
        }

        public bool? CheckColor()
        {
            if (IsKingInCheck(false))
            {
                return false;
            }
            else if (IsKingInCheck(true))
            {
                return true;
            }

            return null;
        }

        private bool IsKingInCheck(bool color)
        {
            BoardSquare? square = GetKing(color);

            if (square is null || square.Piece is null || square.Piece is not King king)
            {
                return false;
            }

            return king.InCheck;
        }
    }
}
