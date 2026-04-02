namespace LearningApp.API.DTOs.Admin.Quizzes;

public class AdminQuizListItemResponseDto
{
    public Guid Id { get; set; }

    public Guid TopicId { get; set; }

    public string TopicTitle { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int QuestionCount { get; set; }
}
