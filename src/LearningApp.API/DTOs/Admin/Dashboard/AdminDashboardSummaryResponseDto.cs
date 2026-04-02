namespace LearningApp.API.DTOs.Admin.Dashboard;

public class AdminDashboardSummaryResponseDto
{
    public int TotalUsers { get; set; }

    public int TotalTopics { get; set; }

    public int TotalLessons { get; set; }

    public int TotalQuizzes { get; set; }

    public int TotalAchievements { get; set; }
}
