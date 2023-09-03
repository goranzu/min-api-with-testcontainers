using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api;
using TodoApp.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDbContext>(options =>
{
    options.UseNpgsql("Host=localhost;Database=Todos_Dev;Username=postgres;Password=password");
});


var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/filter/{param}", (string param) => Results.Ok(param)).AddEndpointFilter(async (context, next) =>
{
    var param = context.GetArgument<string>(0);
    if (param != "success")
    {
        return Results.BadRequest();
    }

    return await next(context);
});

app.MapGet("/todos", async ([FromServices] TodoDbContext dbContext) =>
{
    var todos = await dbContext.Todos.ToListAsync();
    return Results.Ok(todos);
});

app.MapGet("/todos/{id:int}", async ([FromServices] TodoDbContext dbContext, int id) =>
{
    var todos = await dbContext.Todos.FindAsync(id);
    if (todos is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(todos);
});

app.MapPost("/todos", async ([FromServices] TodoDbContext dbContext, CreateTodoRequest request) =>
{
    var todo = new Todo() { Title = request.Title };
    await dbContext.Todos.AddAsync(todo);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapPatch("/todos/{id:int}", async ([FromServices] TodoDbContext dbContext, UpdateTodoRequest request, int id) =>
{
    var todo = await dbContext.Todos.FindAsync(id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    if (request.Title is not null)
    {
        todo.Title = request.Title;
    }

    if (request.IsComplete is not null)
    {
        todo.IsComplete = request.IsComplete.Value;
    }

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/todos/{id}", async ([FromServices] TodoDbContext dbContext, int id) =>
{
    var todo = await dbContext.Todos.FindAsync(id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    dbContext.Todos.Remove(todo);

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});


app.Run();

public sealed record CreateTodoRequest(string Title);

public sealed record UpdateTodoRequest(string? Title, bool? IsComplete);

public partial class Program
{
}