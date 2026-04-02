using System.ComponentModel.DataAnnotations;

namespace LearningApp.API.DTOs.Admin.Quizzes;

public class AdminQuizQuestionCreateDto
{
    [Required]
    public string QuestionText { get; set; } = string.Empty;
}
