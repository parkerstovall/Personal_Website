using System.Security.Principal;
using ChessApi.Models.DB;
using ChessApi.Pieces;
using ChessApi.Pieces.Interfaces;

namespace ChessApi.Models.API
{
    public class PossibleMove
    {
        public int[] MoveTo { get; set; } = [-1, -1];
        public int[] MoveFrom { get; set; } = [-1, -1];
        public bool HasMoved
        {
            get
            {
                if (MovingPiece is Pawn)
                {
                    return MoveFrom[0] != 1 && MoveFrom[0] != 6;
                }

                return MoveFrom[0] != 0 && MoveFrom[0] != 7;
            }
        }
        public required IPiece MovingPiece { get; set; }
        public IPiece? CapturedPiece { get; set; }
        public int[]? CapturedMoveToOverride { get; set; }
        public int[]? CapturedMoveFromOverride { get; set; }

        public override string ToString()
        {
            var msg =
                $"{MovingPiece.Color} {MovingPiece.GetType().Name} moves from [{MoveFrom[0]}, {MoveFrom[1]}] to [{MoveTo[0]}, {MoveTo[1]}]";
            if (CapturedPiece is not null)
            {
                msg += $", capturing ${CapturedPiece.Color} {CapturedPiece.GetType().Name}";
            }

            return msg;
        }
    }
}
