namespace LearningApp.Domain.Entities;

public class QuizQuestion
{
    public Guid Id { get; set; }

    public Guid QuizId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public Quiz? Quiz { get; set; }

    public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
}
