using LearningApp.Domain.Entities;
using LearningApp.Infrastructure.Persistence;

namespace LearningApp.Infrastructure.Persistence.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        if (context.Topics.Any())
            return;

        var topic1Id = Guid.NewGuid();
        var topic2Id = Guid.NewGuid();

        var topics = new List<Topic>
        {
            new Topic
            {
                Id = topic1Id,
                Title = "C# Basics",
                Description = "Learn the fundamentals of C#",
                Order = 1
            },
            new Topic
            {
                Id = topic2Id,
                Title = "Algorithms",
                Description = "Learn basic algorithm concepts",
                Order = 2
            }
        };

        var lessons = new List<Lesson>
        {
            new Lesson
            {
                Id = Guid.NewGuid(),
                TopicId = topic1Id,
                Title = "Variables",
                Content = "Variables store data values in C#.",
                Order = 1,
                IsLocked = false
            },
            new Lesson
            {
                Id = Guid.NewGuid(),
                TopicId = topic1Id,
                Title = "Conditions",
                Content = "Conditions let you control program flow.",
                Order = 2,
                IsLocked = false
            },
            new Lesson
            {
                Id = Guid.NewGuid(),
                TopicId = topic2Id,
                Title = "What is an Algorithm?",
                Content = "An algorithm is a step-by-step solution to a problem.",
                Order = 1,
                IsLocked = false
            },
            new Lesson
            {
                Id = Guid.NewGuid(),
                TopicId = topic2Id,
                Title = "Sorting Basics",
                Content = "Sorting arranges items in a particular order.",
                Order = 2,
                IsLocked = false
            }
        };

        await context.Topics.AddRangeAsync(topics);
        await context.Lessons.AddRangeAsync(lessons);
        await context.SaveChangesAsync();
    }
}