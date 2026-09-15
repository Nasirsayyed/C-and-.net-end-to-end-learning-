using TodoApi.Models;

namespace TodoApi.Repositories;

public interface ITodoRepository
{
    IReadOnlyList<TodoItem> GetAll(bool? isComplete);
    TodoItem? GetById(Guid id);
    TodoItem Add(TodoItem item);
    bool Update(TodoItem item);
    bool Delete(Guid id);
}
