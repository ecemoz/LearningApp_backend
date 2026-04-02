namespace LearningApp.API.DTOs.Admin.Quizzes;

public class AdminQuizQuestionResponseDto
{
    public Guid Id { get; set; }

    public Guid QuizId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public List<AdminQuizOptionResponseDto> Options { get; set; } = new();
}
