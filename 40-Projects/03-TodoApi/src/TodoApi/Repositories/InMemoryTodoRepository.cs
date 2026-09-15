using System.Collections.Concurrent;
using TodoApi.Models;

namespace TodoApi.Repositories;

/// <summary>
/// Thread-safe in-memory repository. Registered as a Singleton (see module
/// 18-Dependency-Injection) since it IS the storage — unlike a DbContext-backed
/// repository, there is no per-request state to isolate here.
/// </summary>
public class InMemoryTodoRepository : ITodoRepository
{
    private readonly ConcurrentDictionary<Guid, TodoItem> _items = new();

    public IReadOnlyList<TodoItem> GetAll(bool? isComplete)
    {
        var query = _items.Values.AsEnumerable();
        if (isComplete.HasValue)
        {
            query = query.Where(i => i.IsComplete == isComplete.Value);
        }
        return query.OrderBy(i => i.CreatedAtUtc).ToList();
    }

    public TodoItem? GetById(Guid id) => _items.GetValueOrDefault(id);

    public TodoItem Add(TodoItem item)
    {
        _items[item.Id] = item;
        return item;
    }

    public bool Update(TodoItem item)
    {
        if (!_items.ContainsKey(item.Id))
        {
            return false;
        }
        _items[item.Id] = item;
        return true;
    }

    public bool Delete(Guid id) => _items.TryRemove(id, out _);
}
