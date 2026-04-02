namespace LearningApp.API.DTOs.Admin.Quizzes;

public class AdminQuizOptionResponseDto
{
    public Guid Id { get; set; }

    public Guid QuizQuestionId { get; set; }

    public string OptionText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
