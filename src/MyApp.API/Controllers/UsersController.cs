namespace MyApp.API.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.API.Data;
using MyApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using MyApp.API.DTOs;
using MyApp.API.Services;

[ApiController]
[Authorize]
[Route("api/[controller]")] // [controller] là mã giả sẽ được thay bằng tên của controller là: api/users
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;

    public UsersController(AppDbContext db, IAuthorizationService authorizationService, IUserService userService)
    {
        _context = db;
        _authorizationService = authorizationService;
        _userService = userService;
    }

    // GET: api/users
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Username
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET: api/users/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, id, "CanUpdateUser");

        if (!authorizationResult.Succeeded)
            return Forbid();

        var user = await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Username
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // POST: api/users
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser(CreateUserDto input)
    {
        try
        {
            var user = await _userService.CreateUserAsync(input);

            var result = new
            {
                user.Id,
                user.Name,
                user.Username,
                user.Role
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT: api/users/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto input)
    {
        var authorizationResult = await _authorizationService.AuthorizeAsync(User, id, "CanUpdateUser");

        if (!authorizationResult.Succeeded)
            return Forbid();

        var success = await _userService.UpdateUserAsync(id, input);

        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUserRole(int id, UpdateUserRoleDto input)
    {
        var success = await _userService.UpdateUserRoleAsync(id, input);

        if (!success)
            return NotFound();

        return NoContent();
    }

    // DELETE: api/users/1
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/users/search?name=John&page=1
    [HttpGet("search")]
    public IActionResult Search(string name, int page = 1)
    {
        Console.WriteLine($"name = '{name}'");
        var pageSize = 10;
        var users = _context.Users
            .Where(u => u.Name.Contains(name))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(users);
    }

    // POST: api/users/upload
    [HttpPost("upload")]
    public IActionResult Upload([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, file.FileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        return Ok(new { filePath });
    }
}