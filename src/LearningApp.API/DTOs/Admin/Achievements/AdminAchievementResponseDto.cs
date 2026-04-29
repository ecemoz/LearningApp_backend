namespace LearningApp.API.DTOs.Admin.Achievements;

public class AdminAchievementResponseDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid? TopicId { get; set; }

    public string? TopicTitle { get; set; }
}
