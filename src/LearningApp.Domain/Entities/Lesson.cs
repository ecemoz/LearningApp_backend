namespace LearningApp.Domain.Entities 

public class Lesson {
   public Guid Id { get; set; }
   public Guid TopicId { get; set; }
   public string Title { get; set; } = string.Empty;
   public string Content { get; set; } = string.Empty;
   public int Order { get; set; } // Bir ders sıralaması için ...
   public bool isLocked { get; set; } = false; 

   public Topic? Topic { get; set; };
   public ICollection<UserLessonProgress> UserProgresses { get; set; } = new List<UserLessonProgress>();

}