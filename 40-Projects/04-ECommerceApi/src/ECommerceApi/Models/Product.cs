namespace ECommerceApi.Models;

public class Product
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
