namespace LearningApp.API.DTOs.Admin.Quizzes;

public class AdminQuizResponseDto
{
    public Guid Id { get; set; }

    public Guid TopicId { get; set; }

    public string TopicTitle { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public List<AdminQuizQuestionResponseDto> Questions { get; set; } = new();
}
