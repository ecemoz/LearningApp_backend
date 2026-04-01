namespace LearningApp.API.DTOs.Quiz;

public class QuizResponseDto
{
    public Guid Id { get; set; }

    public Guid TopicId { get; set; }

    public string Title { get; set; } = string.Empty;

    public List<QuizQuestionResponseDto> Questions { get; set; } = new();
}
