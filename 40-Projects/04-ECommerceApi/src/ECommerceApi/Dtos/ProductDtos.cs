using ECommerceApi.Models;

namespace ECommerceApi.Dtos;

public record CreateProductRequest(string Name, decimal Price, int StockQuantity);

public record ProductResponse(Guid Id, string Name, decimal Price, int StockQuantity)
{
    public static ProductResponse FromModel(Product p) => new(p.Id, p.Name, p.Price, p.StockQuantity);
}
