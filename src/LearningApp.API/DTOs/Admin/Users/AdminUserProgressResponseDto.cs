namespace LearningApp.API.DTOs.Admin.Users;

public class AdminUserProgressResponseDto
{
    public int TotalLessons { get; set; }

    public int CompletedLessons { get; set; }

    public int Percentage { get; set; }

    public List<AdminUserTopicProgressResponseDto> TopicProgresses { get; set; } = new();
}