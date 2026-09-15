using System.Collections.Concurrent;
using OrdersService.Models;

namespace OrdersService.Storage;

public class InMemoryOrderStore : IOrderStore
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public void Add(Order order) => _orders[order.Id] = order;

    public Order? GetById(Guid id) => _orders.GetValueOrDefault(id);
}
