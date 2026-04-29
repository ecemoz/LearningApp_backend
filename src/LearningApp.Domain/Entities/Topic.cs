namespace LearningApp.Domain.Entities;

public class Topic
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Order { get; set; } // Bir konu sıralaması için ...

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
}