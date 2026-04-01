namespace LearningApp.Domain.Entities;

public class UserQuizAttempt
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid QuizId { get; set; }

    public int Score { get; set; }

    public int CorrectCount { get; set; }

    public int TotalQuestionCount { get; set; }

    public DateTime AttemptedAt { get; set; }

    public User? User { get; set; }

    public Quiz? Quiz { get; set; }
}
