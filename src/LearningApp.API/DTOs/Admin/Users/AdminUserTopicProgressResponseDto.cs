namespace LearningApp.API.DTOs.Admin.Users;

public class AdminUserTopicProgressResponseDto
{
    public Guid TopicId { get; set; }

    public string TopicTitle { get; set; } = string.Empty;

    public int TotalLessons { get; set; }

    public int CompletedLessons { get; set; }

    public int Percentage { get; set; }
}