using FM100.Core.Management;

namespace FM100.UnitTest.Core.Management;

public class TableSortServiceTests
{
    [Fact]
    public void Sort_OrdersAscendingByRequestedColumn()
    {
        var rows = new[]
        {
            new DemoRow("Juventus", 71),
            new DemoRow("Milan", 63),
            new DemoRow("Atalanta", 68)
        };

        var sorted = TableSortService.Sort(rows, row => row.Points, TableSortDirection.Ascending);

        Assert.Equal(["Milan", "Atalanta", "Juventus"], sorted.Select(row => row.Name));
    }

    [Fact]
    public void Sort_OrdersDescendingByRequestedColumn()
    {
        var rows = new[]
        {
            new DemoRow("Juventus", 71),
            new DemoRow("Milan", 63),
            new DemoRow("Atalanta", 68)
        };

        var sorted = TableSortService.Sort(rows, row => row.Name, TableSortDirection.Descending);

        Assert.Equal(["Milan", "Juventus", "Atalanta"], sorted.Select(row => row.Name));
    }

    private sealed record DemoRow(string Name, int Points);
}
