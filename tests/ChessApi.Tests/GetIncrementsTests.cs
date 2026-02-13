using ChessApi.HelperClasses.Chess;

namespace ChessApi.Tests;

public class GetIncrementsTests
{
    [Fact]
    public void NoPinQueenDirections_Returns8Directions()
    {
        var result = PieceHelper.GetIncrements(Direction.None, diag: true, straight: true);

        Assert.Equal(8, result.Item1.Length);
        Assert.Equal(8, result.Item2.Length);
    }

    [Fact]
    public void NoPinBishopDirections_Returns4DiagonalDirections()
    {
        var result = PieceHelper.GetIncrements(Direction.None, diag: true, straight: false);

        Assert.Equal(4, result.Item1.Length);
        Assert.Equal(4, result.Item2.Length);
    }

    [Fact]
    public void NoPinRookDirections_Returns4StraightDirections()
    {
        var result = PieceHelper.GetIncrements(Direction.None, diag: false, straight: true);

        Assert.Equal(4, result.Item1.Length);
        Assert.Equal(4, result.Item2.Length);
    }

    [Fact]
    public void PinnedDiagonalTLBR_Returns2Directions()
    {
        var result = PieceHelper.GetIncrements(
            Direction.FromTopLeftToBottomRight,
            diag: true,
            straight: true
        );

        Assert.Equal(2, result.Item1.Length);
        Assert.Equal(2, result.Item2.Length);
        // Should be (1,1) and (-1,-1)
        Assert.Equal(1, result.Item1[0]);
        Assert.Equal(1, result.Item2[0]);
        Assert.Equal(-1, result.Item1[1]);
        Assert.Equal(-1, result.Item2[1]);
    }

    [Fact]
    public void PinnedStraightTB_Returns2Directions()
    {
        var result = PieceHelper.GetIncrements(
            Direction.FromTopToBottom,
            diag: false,
            straight: true
        );

        Assert.Equal(2, result.Item1.Length);
        Assert.Equal(2, result.Item2.Length);
        // Should be (-1,0) and (1,0)
        Assert.Equal(-1, result.Item1[0]);
        Assert.Equal(0, result.Item2[0]);
        Assert.Equal(1, result.Item1[1]);
        Assert.Equal(0, result.Item2[1]);
    }

    [Fact]
    public void PinnedStraightLR_Returns2Directions()
    {
        var result = PieceHelper.GetIncrements(
            Direction.FromLeftToRight,
            diag: false,
            straight: true
        );

        Assert.Equal(2, result.Item1.Length);
        Assert.Equal(2, result.Item2.Length);
        // Should be (0,-1) and (0,1)
        Assert.Equal(0, result.Item1[0]);
        Assert.Equal(-1, result.Item2[0]);
        Assert.Equal(0, result.Item1[1]);
        Assert.Equal(1, result.Item2[1]);
    }

    [Fact]
    public void ReturnsSameArrayInstances_ForSameInput()
    {
        // Static readonly arrays should return the same references each time
        var result1 = PieceHelper.GetIncrements(Direction.None, diag: true, straight: true);
        var result2 = PieceHelper.GetIncrements(Direction.None, diag: true, straight: true);

        Assert.Same(result1.Item1, result2.Item1);
        Assert.Same(result1.Item2, result2.Item2);
    }

    [Fact]
    public void PinnedBishopOnStraightLine_ReturnsEmpty()
    {
        // Bishop pinned on a straight line can't move at all
        var result = PieceHelper.GetIncrements(
            Direction.FromTopToBottom,
            diag: true,
            straight: false
        );

        Assert.Empty(result.Item1);
        Assert.Empty(result.Item2);
    }

    [Fact]
    public void PinnedRookOnDiagonal_ReturnsEmpty()
    {
        // Rook pinned on a diagonal can't move at all
        var result = PieceHelper.GetIncrements(
            Direction.FromTopLeftToBottomRight,
            diag: false,
            straight: true
        );

        Assert.Empty(result.Item1);
        Assert.Empty(result.Item2);
    }
}
