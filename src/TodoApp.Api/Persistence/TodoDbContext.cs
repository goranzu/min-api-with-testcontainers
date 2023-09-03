using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace TodoApp.Api.Persistence;

public sealed class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(TodoDbContext)) ??
                                                     throw new InvalidOperationException());
    }
}