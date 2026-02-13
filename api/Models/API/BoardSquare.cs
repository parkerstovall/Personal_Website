using ChessApi.Pieces.Interfaces;

namespace ChessApi.Models.API
{
    public class BoardSquare
    {
        public IPiece? Piece = null;
        public int[] Coords { get; set; } = [-1, -1];
        public bool? CheckBlockingColor { get; set; } = null;
        public bool? EnPassantColor = null;
        public int BlackPressure { get; set; }
        public int WhitePressure { get; set; }
        public bool WhiteKingPressure { get; set; }
        public bool BlackKingPressure { get; set; }

        public BoardSquare Copy()
        {
            BoardSquare copy = new()
            {
                Piece = Piece?.Copy(),
                Coords = Coords,
                CheckBlockingColor = CheckBlockingColor,
                BlackPressure = BlackPressure,
                WhitePressure = WhitePressure,
                EnPassantColor = EnPassantColor,
            };
            return copy;
        }
    }
}
