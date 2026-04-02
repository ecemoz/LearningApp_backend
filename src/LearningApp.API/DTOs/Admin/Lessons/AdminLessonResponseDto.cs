namespace LearningApp.API.DTOs.Admin.Lessons;

public class AdminLessonResponseDto
{
    public Guid Id { get; set; }

    public Guid TopicId { get; set; }

    public string TopicTitle { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int Order { get; set; }

    public bool IsLocked { get; set; }
}
