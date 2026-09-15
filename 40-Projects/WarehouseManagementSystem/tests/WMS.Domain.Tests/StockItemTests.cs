using WMS.Domain;
using WMS.Domain.Exceptions;
using Xunit;

namespace WMS.Domain.Tests;

public class StockItemTests
{
    [Fact]
    public void Constructor_WithValidArguments_SetsInitialState()
    {
        var item = new StockItem("SKU-1", "WH-1", initialQuantity: 100);

        Assert.Equal(100, item.QuantityOnHand);
        Assert.Equal(0, item.QuantityReserved);
        Assert.Equal(100, item.Available);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptySku_Throws(string sku)
    {
        Assert.Throws<ArgumentException>(() => new StockItem(sku, "WH-1"));
    }

    [Fact]
    public void Constructor_WithNegativeInitialQuantity_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new StockItem("SKU-1", "WH-1", -1));
    }

    [Fact]
    public void Receive_IncreasesQuantityOnHand()
    {
        var item = new StockItem("SKU-1", "WH-1", 10);
        item.Receive(5);
        Assert.Equal(15, item.QuantityOnHand);
    }

    [Fact]
    public void Receive_WithNonPositiveQuantity_Throws()
    {
        var item = new StockItem("SKU-1", "WH-1", 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => item.Receive(0));
    }

    [Fact]
    public void Reserve_WithinAvailableQuantity_Succeeds()
    {
        var item = new StockItem("SKU-1", "WH-1", 10);
        item.Reserve(4);

        Assert.Equal(4, item.QuantityReserved);
        Assert.Equal(6, item.Available);
    }

    [Fact]
    public void Reserve_ExceedingAvailableQuantity_ThrowsInsufficientStockException()
    {
        var item = new StockItem("SKU-1", "WH-1", 10);

        var ex = Assert.Throws<InsufficientStockException>(() => item.Reserve(11));

        Assert.Equal("SKU-1", ex.Sku);
        Assert.Equal(11, ex.Requested);
        Assert.Equal(10, ex.Available);
    }

    [Fact]
    public void Reserve_CannotOverdraw_EvenAcrossMultipleCalls()
    {
        // This is the invariant the aggregate exists to protect: no external code path
        // can ever push QuantityReserved past QuantityOnHand, no matter how it's called.
        var item = new StockItem("SKU-1", "WH-1", 10);
        item.Reserve(6);

        Assert.Throws<InsufficientStockException>(() => item.Reserve(5)); // only 4 left, not 5
        Assert.Equal(6, item.QuantityReserved); // the failed attempt must not have partially applied
    }

    [Fact]
    public void Release_ReducesReservedQuantity_WithoutChangingOnHand()
    {
        var item = new StockItem("SKU-1", "WH-1", 10);
        item.Reserve(6);

        item.Release(4);

        Assert.Equal(2, item.QuantityReserved);
        Assert.Equal(10, item.QuantityOnHand);
        Assert.Equal(8, item.Available);
    }

    [Fact]
    public void Release_MoreThanReserved_Throws()
    {
        var item = new StockItem("SKU-1", "WH-1", 10);
        item.Reserve(3);

        Assert.Throws<InvalidOperationException>(() => item.Release(4));
    }

    [Fact]
    public void ShipReserved_ReducesBothOnHandAndReserved()
    {
        var item = new StockItem("SKU-1", "WH-1", 10);
        item.Reserve(5);

        item.ShipReserved(5);

        Assert.Equal(5, item.QuantityOnHand);
        Assert.Equal(0, item.QuantityReserved);
    }

    [Fact]
    public void ShipReserved_MoreThanReserved_Throws()
    {
        var item = new StockItem("SKU-1", "WH-1", 10);
        item.Reserve(2);

        Assert.Throws<InvalidOperationException>(() => item.ShipReserved(3));
    }

    [Fact]
    public void Available_NeverGoesNegative_AcrossFullLifecycle()
    {
        var item = new StockItem("SKU-1", "WH-1", 20);
        item.Reserve(20);

        Assert.Equal(0, item.Available);
        Assert.Throws<InsufficientStockException>(() => item.Reserve(1));
    }
}
