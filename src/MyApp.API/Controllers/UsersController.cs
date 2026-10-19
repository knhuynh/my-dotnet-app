namespace MyApp.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.API.Data;
using MyApp.API.Models;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Authorize]
[Route("api/[controller]")] // [controller] là mã giả sẽ được thay bằng tên của controller là: api/users
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/users
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
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
        var user = await _db.Users
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
    public async Task<IActionResult> CreateUser(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // PUT: api/users/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, User input)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        user.Name = input.Name;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/users/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/users/search?name=John&page=1
    [HttpGet("search")]
    public IActionResult Search(string name, int page = 1)
    {
        Console.WriteLine($"name = '{name}'");
        var pageSize = 10;
        var users = _db.Users
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