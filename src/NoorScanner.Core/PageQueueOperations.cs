namespace NoorScanner.Core;

/// <summary>Small, validated operations for the user's export-order page queue.</summary>
public static class PageQueueOperations
{
    /// <summary>Moves one item to a zero-based destination index while preserving all other items.</summary>
    public static bool TryMove<T>(IList<T> items, int sourceIndex, int destinationIndex)
    {
        ArgumentNullException.ThrowIfNull(items);
        if ((uint)sourceIndex >= (uint)items.Count || (uint)destinationIndex >= (uint)items.Count)
        {
            return false;
        }

        if (sourceIndex == destinationIndex) return true;
        var item = items[sourceIndex];
        items.RemoveAt(sourceIndex);
        items.Insert(destinationIndex, item);
        return true;
    }

    /// <summary>Removes an item by zero-based index and returns it; invalid indexes leave the list unchanged.</summary>
    public static bool TryRemoveAt<T>(IList<T> items, int index, out T removed) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(items);
        if ((uint)index >= (uint)items.Count)
        {
            removed = default!;
            return false;
        }

        removed = items[index];
        items.RemoveAt(index);
        return true;
    }
}
