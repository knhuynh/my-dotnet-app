using MyApp.API.Data;
using MyApp.API.DTOs;
using MyApp.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MyApp.API.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<User> CreateUserAsync(CreateUserDto input)
    {
        var role = input.Role.Trim();

        if (role != "Admin" && role != "User")
        {
            throw new ArgumentException("Role không hợp lệ.");
        }

        var username = input.Username.Trim();

        var existingUser = await _context.Users
            .AnyAsync(u => u.Username == username);

        if (existingUser)
        {
            throw new InvalidOperationException("Username đã tồn tại.");
        }

        var user = new User
        {
            Name = input.Name.Trim(),
            Username = username,
            Role = role
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            input.Password
        );

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
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

    public async Task<bool> UpdateUserRoleAsync(
        int id,
        UpdateUserRoleDto input)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return false;

        user.Role = input.Role;

        await _context.SaveChangesAsync();

        return true;
    }
}