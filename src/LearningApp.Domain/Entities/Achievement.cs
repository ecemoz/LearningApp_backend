namespace LearningApp.Domain.Entities;

public class Achievement
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
