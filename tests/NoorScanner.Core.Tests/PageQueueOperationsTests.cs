using NoorScanner.Core;
using Xunit;

namespace NoorScanner.Core.Tests;

public sealed class PageQueueOperationsTests
{
    [Theory]
    [InlineData(0, 2, "b,c,a")]
    [InlineData(2, 0, "c,a,b")]
    [InlineData(1, 2, "a,c,b")]
    public void TryMove_ChangesOnlyTheRequestedQueueOrder(int sourceIndex, int destinationIndex, string expected)
    {
        var pages = new List<string> { "a", "b", "c" };

        var moved = PageQueueOperations.TryMove(pages, sourceIndex, destinationIndex);

        Assert.True(moved);
        Assert.Equal(expected.Split(','), pages);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(3, 0)]
    [InlineData(0, 3)]
    public void TryMove_InvalidIndexesDoNotMutateTheQueue(int sourceIndex, int destinationIndex)
    {
        var pages = new List<string> { "a", "b", "c" };

        var moved = PageQueueOperations.TryMove(pages, sourceIndex, destinationIndex);

        Assert.False(moved);
        Assert.Equal(new[] { "a", "b", "c" }, pages);
    }

    [Fact]
    public void TryMove_SameIndexIsASuccessfulNoOp()
    {
        var pages = new List<string> { "a", "b", "c" };

        var moved = PageQueueOperations.TryMove(pages, 1, 1);

        Assert.True(moved);
        Assert.Equal(new[] { "a", "b", "c" }, pages);
    }

    [Fact]
    public void TryRemoveAt_ReturnsTheRemovedItemAndKeepsRemainingOrder()
    {
        var pages = new List<string> { "a", "b", "c" };

        var removed = PageQueueOperations.TryRemoveAt(pages, 1, out var page);

        Assert.True(removed);
        Assert.Equal("b", page);
        Assert.Equal(new[] { "a", "c" }, pages);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void TryRemoveAt_InvalidIndexesDoNotMutateTheQueue(int index)
    {
        var pages = new List<string> { "a", "b", "c" };

        var removed = PageQueueOperations.TryRemoveAt(pages, index, out var page);

        Assert.False(removed);
        Assert.Null(page);
        Assert.Equal(new[] { "a", "b", "c" }, pages);
    }
}
