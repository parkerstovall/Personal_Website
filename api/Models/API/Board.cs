using System.Text;

namespace ChessApi.Models.API
{
    public class Board
    {
        public List<BoardRow> Rows { get; set; } = [];

        public override string ToString()
        {
            var sb = new StringBuilder(Rows.Count * 17); // ~17 chars per row
            foreach (BoardRow row in Rows)
            {
                foreach (BoardSquare square in row.Squares)
                {
                    if (square.Piece is null)
                    {
                        sb.Append("0 ");
                    }
                    else
                    {
                        string type = square.Piece.GetType().Name;
                        if (type == "King")
                        {
                            type = "n";
                        }

                        sb.Append(!square.Piece.Color ? type.ToUpper()[0] : type.ToLower()[0]);
                        sb.Append(' ');
                    }
                }
                sb.Append('\n');
            }
            return sb.ToString();
        }
    }
}
