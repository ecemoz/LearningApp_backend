namespace LearningApp.Domain.Entities
{
    public class Lesson
    {
        public Guid Id { get; set; }

        public Guid TopicId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int Order { get; set; }

        public bool IsLocked { get; set; } = false;
        
        public Topic? Topic { get; set; }

        public ICollection<UserLessonProgress> UserLessonProgresses { get; set; } = new List<UserLessonProgress>();
    }
}