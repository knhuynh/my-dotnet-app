using Microsoft.EntityFrameworkCore;
using MyApp.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Print the current environment to the console
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // if use this to up line https in launchSettings.json to first

app.MapGet("/", () => "Hello .NET Docker Container Hot Reload!");

app.MapGet("/users", async (AppDbContext db) =>
{
    return await db.Users.ToListAsync();
});

app.MapControllers();

app.Run();
