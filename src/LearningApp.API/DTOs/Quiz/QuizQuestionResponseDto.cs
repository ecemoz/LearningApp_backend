namespace LearningApp.API.DTOs.Quiz;

public class QuizQuestionResponseDto
{
    public Guid Id { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public List<QuizOptionResponseDto> Options { get; set; } = new();
}
