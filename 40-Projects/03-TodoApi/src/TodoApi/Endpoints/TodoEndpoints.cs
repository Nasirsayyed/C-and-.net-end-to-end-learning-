using Microsoft.AspNetCore.Http.HttpResults;
using TodoApi.Dtos;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Endpoints;

/// <summary>
/// Minimal API endpoints for Todo CRUD. See module 19-Web-API for the reasoning behind
/// DTOs at the boundary, correct status codes, and ProblemDetails-based validation errors.
/// </summary>
public static class TodoEndpoints
{
    public static RouteGroupBuilder MapTodoEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:guid}", Update);
        group.MapDelete("/{id:guid}", Delete);
        return group;
    }

    private static Ok<List<TodoResponse>> GetAll(ITodoRepository repository, bool? isComplete)
    {
        var items = repository.GetAll(isComplete).Select(TodoResponse.FromModel).ToList();
        return TypedResults.Ok(items);
    }

    private static Results<Ok<TodoResponse>, NotFound> GetById(ITodoRepository repository, Guid id)
    {
        var item = repository.GetById(id);
        return item is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(TodoResponse.FromModel(item));
    }

    private static Results<Created<TodoResponse>, ValidationProblem> Create(
        ITodoRepository repository, CreateTodoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Title"] = ["Title is required and must be 1-200 characters."]
            });
        }

        var item = repository.Add(new TodoItem { Title = request.Title });
        var response = TodoResponse.FromModel(item);
        return TypedResults.Created($"/api/todos/{item.Id}", response);
    }

    private static Results<NoContent, NotFound, ValidationProblem> Update(
        ITodoRepository repository, Guid id, UpdateTodoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Title"] = ["Title is required and must be 1-200 characters."]
            });
        }

        var existing = repository.GetById(id);
        if (existing is null)
        {
            return TypedResults.NotFound();
        }

        existing.Title = request.Title;
        existing.IsComplete = request.IsComplete;
        repository.Update(existing);
        return TypedResults.NoContent();
    }

    private static Results<NoContent, NotFound> Delete(ITodoRepository repository, Guid id) =>
        repository.Delete(id) ? TypedResults.NoContent() : TypedResults.NotFound();
}
