namespace LearningApp.Domain.Entities;

public class Quiz
{
    public Guid Id { get; set; }

    public Guid TopicId { get; set; }

    public string Title { get; set; } = string.Empty;

    public Topic? Topic { get; set; }

    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();

    public ICollection<UserQuizAttempt> UserQuizAttempts { get; set; } = new List<UserQuizAttempt>();
}
