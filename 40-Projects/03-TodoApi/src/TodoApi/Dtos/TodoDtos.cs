using System.ComponentModel.DataAnnotations;
using TodoApi.Models;

namespace TodoApi.Dtos;

public record CreateTodoRequest([Required, StringLength(200, MinimumLength = 1)] string Title);

public record UpdateTodoRequest([Required, StringLength(200, MinimumLength = 1)] string Title, bool IsComplete);

public record TodoResponse(Guid Id, string Title, bool IsComplete, DateTimeOffset CreatedAtUtc)
{
    public static TodoResponse FromModel(TodoItem item) =>
        new(item.Id, item.Title, item.IsComplete, item.CreatedAtUtc);
}
