using System.ComponentModel.DataAnnotations;

namespace LearningApp.API.DTOs.Admin.Quizzes;

public class AdminQuizUpdateDto
{
    [Required]
    public Guid TopicId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;
}
