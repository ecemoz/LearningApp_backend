using System.ComponentModel.DataAnnotations;

namespace LearningApp.API.DTOs.Admin.Achievements;

public class AdminAchievementUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public Guid? TopicId { get; set; }
}
