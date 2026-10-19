namespace MyApp.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.API.Data;
using MyApp.API.DTOs;
using MyApp.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthController(
        AppDbContext context,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        // 1. Kiểm tra username đã tồn tại chưa
        var username = request.Username.Trim();

        var existingUser = await _context.Users
            .AnyAsync(u => u.Username == username);

        if (existingUser)
        {
            return Conflict(new
            {
                message = "Username already exists."
            });
        }

        // 2. Tạo đối tượng User
        var user = new User
        {
            Name = request.Name.Trim(),
            Username = username
        };

        // 3. Băm mật khẩu
        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password
        );

        // 4. Lưu vào database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 5. Trả kết quả
        return StatusCode(201, new
        {
            message = "Registration successful.",
            user = new
            {
                user.Id,
                user.Name,
                user.Username
            }
        });
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        // 1. Tìm tài khoản theo username
        var username = request.Username.Trim();

        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Username == username);

        // 2. Kiểm tra tài khoản
        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        // 3. Xác minh mật khẩu
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        // 4. Lấy cấu hình JWT
        var jwtKey = _configuration["Jwt:Key"]!;
        var jwtIssuer = _configuration["Jwt:Issuer"]!;
        var jwtAudience = _configuration["Jwt:Audience"]!;

        var expiresInMinutes = int.Parse(
            _configuration["Jwt:ExpiresInMinutes"] ?? "60"
        );

        // 5. Tạo claims cho người dùng
        var claims = new[]
        {
        new Claim(
            ClaimTypes.NameIdentifier,
            user.Id.ToString()
        ),
        new Claim(
            ClaimTypes.Name,
            user.Username
        )
    };

        // 6. Tạo khóa ký token
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        // 7. Tạo JWT
        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler()
            .WriteToken(token);

        // 8. Trả kết quả
        return Ok(new
        {
            accessToken,
            tokenType = "Bearer",
            expiresIn = expiresInMinutes
        });
    }
}