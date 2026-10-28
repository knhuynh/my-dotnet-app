using MyApp.API.DTOs;
using MyApp.API.Models;

namespace MyApp.API.Services;

public interface IUserService
{
    Task<User> CreateUserAsync(CreateUserDto input);
    Task<bool> UpdateUserAsync(int id, UpdateUserDto input);
    Task<bool> UpdateUserRoleAsync(int id, UpdateUserRoleDto input);
    Task<PagedResultDto<UserResponseDto>> GetUsersAsync(int page,int pageSize);
}