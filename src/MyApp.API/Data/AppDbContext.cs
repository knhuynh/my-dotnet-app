namespace MyApp.API.Data;

using Microsoft.EntityFrameworkCore;
using MyApp.API.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users { get; set; }

    public DbSet<TodoItem> TodoItems { get; set; }
}