namespace MyApp.API.Data;

using Microsoft.EntityFrameworkCore;
using MyApp.API.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
}