namespace FM100.Core.Management;

public static class TableSortService
{
    public static IReadOnlyList<T> Sort<T>(
        IEnumerable<T> rows,
        Func<T, IComparable?> keySelector,
        TableSortDirection direction)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(keySelector);

        return direction == TableSortDirection.Descending
            ? rows.OrderByDescending(keySelector).ToList()
            : rows.OrderBy(keySelector).ToList();
    }

    public static IReadOnlyList<T> Sort<T, TKey>(
        IEnumerable<T> rows,
        Func<T, TKey> keySelector,
        TableSortDirection direction)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(keySelector);

        return direction == TableSortDirection.Descending
            ? rows.OrderByDescending(keySelector).ToList()
            : rows.OrderBy(keySelector).ToList();
    }
}
