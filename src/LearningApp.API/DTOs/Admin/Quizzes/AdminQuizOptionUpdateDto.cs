using System.ComponentModel.DataAnnotations;

namespace LearningApp.API.DTOs.Admin.Quizzes;

public class AdminQuizOptionUpdateDto
{
    [Required]
    public string OptionText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
