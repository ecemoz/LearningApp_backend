namespace LearningApp.API.DTOs.Quiz;

public class QuizResultResponseDto
{
    public Guid QuizId { get; set; }

    public int Score { get; set; }

    public int CorrectCount { get; set; }

    public int TotalQuestionCount { get; set; }

    public string Message { get; set; } = string.Empty;
}
