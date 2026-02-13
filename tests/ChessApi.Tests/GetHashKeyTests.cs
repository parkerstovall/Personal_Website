using ChessApi.Pieces;

namespace ChessApi.Tests;

public class GetHashKeyTests
{
    [Theory]
    [InlineData(true, "p0")] // black pawn
    [InlineData(false, "p1")] // white pawn
    public void Pawn_GetHashKey_ReturnsCorrectKey(bool color, string expected)
    {
        var pawn = new Pawn(color);
        Assert.Equal(expected, pawn.GetHashKey());
    }

    [Theory]
    [InlineData(true, "r0")]
    [InlineData(false, "r1")]
    public void Rook_GetHashKey_ReturnsCorrectKey(bool color, string expected)
    {
        var rook = new Rook(color);
        Assert.Equal(expected, rook.GetHashKey());
    }

    [Theory]
    [InlineData(true, "n0")]
    [InlineData(false, "n1")]
    public void Knight_GetHashKey_ReturnsCorrectKey(bool color, string expected)
    {
        var knight = new Knight(color);
        Assert.Equal(expected, knight.GetHashKey());
    }

    [Theory]
    [InlineData(true, "b0")]
    [InlineData(false, "b1")]
    public void Bishop_GetHashKey_ReturnsCorrectKey(bool color, string expected)
    {
        var bishop = new Bishop(color);
        Assert.Equal(expected, bishop.GetHashKey());
    }

    [Theory]
    [InlineData(true, "q0")]
    [InlineData(false, "q1")]
    public void Queen_GetHashKey_ReturnsCorrectKey(bool color, string expected)
    {
        var queen = new Queen(color);
        Assert.Equal(expected, queen.GetHashKey());
    }

    [Theory]
    [InlineData(true, "k0")]
    [InlineData(false, "k1")]
    public void King_GetHashKey_ReturnsCorrectKey(bool color, string expected)
    {
        var king = new King(color);
        Assert.Equal(expected, king.GetHashKey());
    }

    [Fact]
    public void GetHashKey_ReturnsSameInstance_NotNewString()
    {
        // Verify the static string optimization: same piece type + color
        // should return the exact same string reference
        var pawn1 = new Pawn(true);
        var pawn2 = new Pawn(true);

        Assert.Same(pawn1.GetHashKey(), pawn2.GetHashKey());
    }

    [Fact]
    public void GetHashKey_DifferentColors_ReturnDifferentKeys()
    {
        var whitePawn = new Pawn(false);
        var blackPawn = new Pawn(true);

        Assert.NotEqual(whitePawn.GetHashKey(), blackPawn.GetHashKey());
    }

    [Fact]
    public void AllPieceTypes_HaveUniqueHashKeys()
    {
        var keys = new HashSet<string>();

        // All piece types, both colors
        keys.Add(new Pawn(true).GetHashKey());
        keys.Add(new Pawn(false).GetHashKey());
        keys.Add(new Rook(true).GetHashKey());
        keys.Add(new Rook(false).GetHashKey());
        keys.Add(new Knight(true).GetHashKey());
        keys.Add(new Knight(false).GetHashKey());
        keys.Add(new Bishop(true).GetHashKey());
        keys.Add(new Bishop(false).GetHashKey());
        keys.Add(new Queen(true).GetHashKey());
        keys.Add(new Queen(false).GetHashKey());
        keys.Add(new King(true).GetHashKey());
        keys.Add(new King(false).GetHashKey());

        Assert.Equal(12, keys.Count);
    }
}
