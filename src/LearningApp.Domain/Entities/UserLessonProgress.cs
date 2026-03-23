namespace LearningApp.Domain.Entities;

public class UserLessonProgress {

    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid LessonId { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedAt { get; set; }

    public User? User { get; set; }

    public Lesson? Lesson { get; set; }

}