using OrdersService.Models;

namespace OrdersService.Storage;

public interface IOrderStore
{
    void Add(Order order);
    Order? GetById(Guid id);
}
