using MyApp.API.DTOs;

namespace MyApp.API.Services;

public interface IUserService
{
    Task<bool> UpdateUserAsync(int id, UpdateUserDto input);
}