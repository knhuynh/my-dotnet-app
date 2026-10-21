using MyApp.API.Data;
using MyApp.API.DTOs;

namespace MyApp.API.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UpdateUserAsync(
        int id,
        UpdateUserDto input)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return false;

        user.Name = input.Name;

        await _context.SaveChangesAsync();

        return true;
    }
}