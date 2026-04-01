namespace LearningApp.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserLessonProgress> LessonProgresses { get; set; } = new List<UserLessonProgress>();

        public ICollection<UserQuizAttempt> QuizAttempts { get; set; } = new List<UserQuizAttempt>();

        public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();

    }
}