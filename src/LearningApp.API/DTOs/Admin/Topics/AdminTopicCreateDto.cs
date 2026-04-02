using System.ComponentModel.DataAnnotations;

namespace LearningApp.API.DTOs.Admin.Topics;

public class AdminTopicCreateDto
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int Order { get; set; }
}
