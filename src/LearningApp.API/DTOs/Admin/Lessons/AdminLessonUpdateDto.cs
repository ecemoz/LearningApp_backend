using System.ComponentModel.DataAnnotations;

namespace LearningApp.API.DTOs.Admin.Lessons;

public class AdminLessonUpdateDto
{
    [Required]
    public Guid TopicId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public int Order { get; set; }

    public bool IsLocked { get; set; }
}
