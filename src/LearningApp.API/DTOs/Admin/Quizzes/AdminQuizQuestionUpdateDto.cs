using System.ComponentModel.DataAnnotations;

namespace LearningApp.API.DTOs.Admin.Quizzes;

public class AdminQuizQuestionUpdateDto
{
    [Required]
    public string QuestionText { get; set; } = string.Empty;
}
