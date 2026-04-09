using Erp.Modules.Sales.Domain;
using Xunit;

namespace Erp.UnitTests;

public class SalesOrderDomainTests
{
    [Fact]
    public void AddLine_ShouldRecalculateTotal()
    {
        var order = new SalesOrder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SO-TEST-001");

        order.AddLine(Guid.NewGuid(), quantity: 2, unitPrice: 50.00m);
        order.AddLine(Guid.NewGuid(), quantity: 1, unitPrice: 25.00m);

        Assert.Equal(125.00m, order.Total);
        Assert.Equal(2, order.Lines.Count);
    }

    [Fact]
    public void NewOrder_ShouldHaveZeroTotal()
    {
        var order = new SalesOrder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SO-TEST-002");

        Assert.Equal(0m, order.Total);
        Assert.Empty(order.Lines);
    }

    [Fact]
    public void AddLine_LineTotal_ShouldEqualQuantityTimesPrice()
    {
        var order = new SalesOrder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SO-TEST-003");

        order.AddLine(Guid.NewGuid(), quantity: 3, unitPrice: 10.50m);

        var line = order.Lines.First();
        Assert.Equal(31.50m, line.LineTotal);
    }
}
