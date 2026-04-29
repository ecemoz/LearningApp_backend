using System.ComponentModel.DataAnnotations;

namespace LearningApp.API.DTOs.Admin.Users;

public class AdminUserRoleUpdateDto
{
    [Required]
    [MaxLength(30)]
    public string Role { get; set; } = string.Empty;
}