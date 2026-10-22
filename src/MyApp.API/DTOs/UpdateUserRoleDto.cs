namespace MyApp.API.DTOs;

using System.ComponentModel.DataAnnotations;

public class UpdateUserRoleDto
{
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "";
}