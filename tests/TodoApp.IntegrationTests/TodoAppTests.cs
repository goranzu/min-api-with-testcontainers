using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TodoApp.Api;
using TodoApp.Api.Persistence;

namespace TodoApp.IntegrationTests;

public class TodoAppTests : IClassFixture<TestWebAppFactory>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly TodoDbContext _dbContext;

    public TodoAppTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    }

    [Fact]
    public async Task Get_EndpointReturnsEmptyArray()
    {
        var response = await _client.GetAsync("todos");

        var todos = await response.Content.DeserializeToListAsync<Todo>();

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8",
            response.Content.Headers.ContentType!.ToString());
        Assert.Empty(todos);
    }

    [Fact]
    public async Task Post_CreatesATodo()
    {
        const string title = "Test Todo";
        var createTodoRequest = new CreateTodoRequest(title);

        var json = JsonConvert.SerializeObject(createTodoRequest);
        var response = await _client.PostAsync("todos", new StringContent(json, Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();

        var todo = await _dbContext.Todos.FirstOrDefaultAsync(x => x.Title == title);
        var todos = await _dbContext.Todos.ToListAsync();
        var todosResponse = await response.Content.DeserializeAsync<Todo>();

        Assert.NotNull(todo);
        Assert.Equal(title, todo.Title);
        Assert.Equal(title, todo.Title);
        Assert.Equal(title, todosResponse?.Title);
        Assert.Single(todos);
    }

    [Fact]
    public async Task GetFilter_ShouldFail()
    {
        var response = await _client.GetAsync($"filter/this-will-fail");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetFilter_ShouldSucceed()
    {
        var response = await _client.GetAsync($"filter/success");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public static class HttpContentExtensions
{
    public static async Task<List<T>> DeserializeToListAsync<T>(this HttpContent content)
    {
        var responseBody = await content.ReadAsStringAsync();

        var items = JsonConvert.DeserializeObject<List<T>>(responseBody);
        return items ?? new List<T>();
    }

    public static async Task<T?> DeserializeAsync<T>(this HttpContent content)
    {
        var responseBody = await content.ReadAsStringAsync();
        var item = JsonConvert.DeserializeObject<T>(responseBody);
        return item;
    }
}