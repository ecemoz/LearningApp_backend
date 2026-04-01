using LearningApp.Domain.Entities;
using LearningApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.Infrastructure.Persistence.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // Seed all base data in an idempotent way so reruns do not duplicate rows.
        var cSharpTopic = await EnsureTopicAsync(
            context,
            "C# Basics",
            "Learn the fundamentals of C#",
            1);

        var algorithmsTopic = await EnsureTopicAsync(
            context,
            "Algorithms",
            "Learn basic algorithm concepts",
            2);

        await EnsureLessonAsync(
            context,
            cSharpTopic.Id,
            "Variables",
            "Variables store data values in C#.",
            1,
            false);

        await EnsureLessonAsync(
            context,
            cSharpTopic.Id,
            "Conditions",
            "Conditions let you control program flow.",
            2,
            false);

        await EnsureLessonAsync(
            context,
            algorithmsTopic.Id,
            "What is an Algorithm?",
            "An algorithm is a step-by-step solution to a problem.",
            1,
            false);

        await EnsureLessonAsync(
            context,
            algorithmsTopic.Id,
            "Sorting Basics",
            "Sorting arranges items in a particular order.",
            2,
            false);

        await EnsureQuizForCSharpBasicsAsync(context, cSharpTopic.Id);
        await EnsureQuizForAlgorithmsAsync(context, algorithmsTopic.Id);

        await EnsureAchievementAsync(
            context,
            "first_lesson",
            "First Lesson Completed",
            "Complete your first lesson");

        await EnsureAchievementAsync(
            context,
            "first_quiz",
            "First Quiz Completed",
            "Complete your first quiz");

        await EnsureAchievementAsync(
            context,
            "topic_complete",
            "Topic Completed",
            "Complete all lessons in a topic");

        await context.SaveChangesAsync();
    }

    private static async Task<Topic> EnsureTopicAsync(
        AppDbContext context,
        string title,
        string description,
        int order)
    {
        var topic = await context.Topics.FirstOrDefaultAsync(t => t.Title == title);
        if (topic is not null)
        {
            return topic;
        }

        topic = new Topic
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Order = order
        };

        await context.Topics.AddAsync(topic);
        return topic;
    }

    private static async Task EnsureLessonAsync(
        AppDbContext context,
        Guid topicId,
        string title,
        string content,
        int order,
        bool isLocked)
    {
        var lessonExists = await context.Lessons
            .AnyAsync(l => l.TopicId == topicId && l.Title == title);

        if (lessonExists)
        {
            return;
        }

        await context.Lessons.AddAsync(new Lesson
        {
            Id = Guid.NewGuid(),
            TopicId = topicId,
            Title = title,
            Content = content,
            Order = order,
            IsLocked = isLocked
        });
    }

    private static async Task EnsureAchievementAsync(
        AppDbContext context,
        string code,
        string title,
        string description)
    {
        var exists = await context.Achievements.AnyAsync(a => a.Code == code);
        if (exists)
        {
            return;
        }

        await context.Achievements.AddAsync(new Achievement
        {
            Id = Guid.NewGuid(),
            Code = code,
            Title = title,
            Description = description
        });
    }

    private static async Task EnsureQuizForCSharpBasicsAsync(AppDbContext context, Guid topicId)
    {
        // Create one MVP quiz for C# Basics if it does not exist yet.
        var quizTitle = "C# Basics Quiz";

        var quizExists = await context.Quizzes.AnyAsync(q => q.TopicId == topicId && q.Title == quizTitle);
        if (quizExists)
        {
            return;
        }

        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            TopicId = topicId,
            Title = quizTitle,
            Questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = "Which keyword is used to declare a variable in C#?",
                    Options = new List<QuizOption>
                    {
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "var", IsCorrect = true },
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "let", IsCorrect = false },
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "define", IsCorrect = false },
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "const", IsCorrect = false }
                    }
                },
                new QuizQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = "Which statement is used for conditional logic in C#?",
                    Options = new List<QuizOption>
                    {
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "if", IsCorrect = true },
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "loop", IsCorrect = false },
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "match", IsCorrect = false },
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "switcher", IsCorrect = false }
                    }
                }
            }
        };

        await context.Quizzes.AddAsync(quiz);
    }

    private static async Task EnsureQuizForAlgorithmsAsync(AppDbContext context, Guid topicId)
    {
        // Create one MVP quiz for Algorithms if it does not exist yet.
        var quizTitle = "Algorithms Quiz";

        var quizExists = await context.Quizzes.AnyAsync(q => q.TopicId == topicId && q.Title == quizTitle);
        if (quizExists)
        {
            return;
        }

        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            TopicId = topicId,
            Title = quizTitle,
            Questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = "What is an algorithm?",
                    Options = new List<QuizOption>
                    {
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "A random guess", IsCorrect = false },
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "A step-by-step solution", IsCorrect = true },
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "A programming language", IsCorrect = false },
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "A database table", IsCorrect = false }
                    }
                },
                new QuizQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = "What is the purpose of sorting?",
                    Options = new List<QuizOption>
                    {
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "To arrange data in order", IsCorrect = true },
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "To delete duplicate values", IsCorrect = false },
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "To encrypt data", IsCorrect = false },
                        new QuizOption { Id = Guid.NewGuid(), OptionText = "To compile code", IsCorrect = false }
                    }
                }
            }
        };

        await context.Quizzes.AddAsync(quiz);
    }
}