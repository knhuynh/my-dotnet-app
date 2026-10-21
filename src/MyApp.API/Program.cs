using Microsoft.EntityFrameworkCore;
using MyApp.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MyApp.API.Authorization;
using MyApp.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Print the current environment to the console
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

// Add services to the container.
// builder.Services.AddControllers(); // Use this if you want to use API only, not MVC
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT Key must contain at least 32 characters."
    );
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("Admin");
    });
    options.AddPolicy("CanUpdateUser", policy =>
    {
        policy.AddRequirements(
            new UpdateUserRequirement()
        );
    });
});

builder.Services.AddScoped<IAuthorizationHandler, UpdateUserAuthorizationHandler>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.Services.GetRequiredService<AppDbContext>().Database.Migrate();

// app.UseLoggingMiddleware();

// app.UseHttpsRedirection(); // if use this to up line https in launchSettings.json to first

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/db-test", async (AppDbContext db) => await db.Database.CanConnectAsync());

app.MapGet("/health", () => Results.Ok("Healthy"));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Todo}/{action=Index}");

app.MapControllers();

app.Run();
