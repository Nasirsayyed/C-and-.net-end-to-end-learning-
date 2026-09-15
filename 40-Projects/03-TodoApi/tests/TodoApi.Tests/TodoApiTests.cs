using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TodoApi.Dtos;
using Xunit;

namespace TodoApi.Tests;

/// <summary>
/// Integration tests spinning up the REAL ASP.NET Core pipeline in-memory
/// (routing, DI, minimal API endpoints) via WebApplicationFactory — see module
/// 34-Testing for why this is closer to production behavior than a pure unit test.
/// </summary>
public class TodoApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TodoApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_WithNoTodos_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/todos");

        response.EnsureSuccessStatusCode();
        var todos = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();
        Assert.NotNull(todos);
    }

    [Fact]
    public async Task Create_WithValidTitle_Returns201WithLocationHeader()
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest("Write integration tests"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.NotNull(created);
        Assert.Equal("Write integration tests", created!.Title);
        Assert.False(created.IsComplete);
    }

    [Fact]
    public async Task Create_WithEmptyTitle_ReturnsValidationProblem()
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ForCreatedTodo_ReturnsIt()
    {
        var created = await CreateTodoAsync("Buy milk");

        var response = await _client.GetAsync($"/api/todos/{created.Id}");

        response.EnsureSuccessStatusCode();
        var fetched = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.Equal(created.Id, fetched!.Id);
    }

    [Fact]
    public async Task GetById_ForUnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/todos/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_MarksTodoComplete()
    {
        var created = await CreateTodoAsync("Finish report");

        var response = await _client.PutAsJsonAsync($"/api/todos/{created.Id}",
            new UpdateTodoRequest(created.Title, IsComplete: true));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var fetched = await _client.GetFromJsonAsync<TodoResponse>($"/api/todos/{created.Id}");
        Assert.True(fetched!.IsComplete);
    }

    [Fact]
    public async Task Update_ForUnknownId_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"/api/todos/{Guid.NewGuid()}",
            new UpdateTodoRequest("Anything", false));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_RemovesTodo()
    {
        var created = await CreateTodoAsync("Temporary");

        var deleteResponse = await _client.DeleteAsync($"/api/todos/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/todos/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithIsCompleteFilter_ReturnsOnlyMatching()
    {
        var incomplete = await CreateTodoAsync("Still pending");
        var complete = await CreateTodoAsync("Already done");
        await _client.PutAsJsonAsync($"/api/todos/{complete.Id}", new UpdateTodoRequest(complete.Title, IsComplete: true));

        var response = await _client.GetFromJsonAsync<List<TodoResponse>>("/api/todos?isComplete=true");

        Assert.NotNull(response);
        Assert.Contains(response!, t => t.Id == complete.Id);
        Assert.DoesNotContain(response!, t => t.Id == incomplete.Id);
    }

    private async Task<TodoResponse> CreateTodoAsync(string title)
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest(title));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TodoResponse>())!;
    }
}
