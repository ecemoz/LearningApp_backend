namespace LearningApp.API.DTOs.Admin.Topics;

public class AdminTopicResponseDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Order { get; set; }

    public int LessonCount { get; set; }

    public int QuizCount { get; set; }
}
