
using System.ComponentModel.DataAnnotations;

namespace MyApp.API.DTOs;

public class CreateUserDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = "";

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = "";

    [Required]
    public string Role { get; set; } = "User";
}
