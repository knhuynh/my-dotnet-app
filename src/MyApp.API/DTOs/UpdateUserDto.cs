namespace MyApp.API.DTOs;

using System.ComponentModel.DataAnnotations;

public class UpdateUserDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";
}