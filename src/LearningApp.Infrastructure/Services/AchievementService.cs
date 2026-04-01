using LearningApp.Domain.Entities;
using LearningApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.Infrastructure.Services;

public class AchievementService
{
    private readonly AppDbContext _context;

    public AchievementService(AppDbContext context)
    {
        _context = context;
    }

    public async Task EnsureFirstLessonAchievementAsync(Guid userId)
    {
        // If user has completed at least one lesson, award first_lesson once.
        var completedLessonCount = await _context.UserLessonProgresses
            .CountAsync(x => x.UserId == userId && x.IsCompleted);

        if (completedLessonCount >= 1)
        {
            await AwardIfMissingAsync(userId, "first_lesson");
        }
    }

    public async Task EnsureFirstQuizAchievementAsync(Guid userId)
    {
        // If user has at least one quiz attempt, award first_quiz once.
        var attemptCount = await _context.UserQuizAttempts
            .CountAsync(x => x.UserId == userId);

        if (attemptCount >= 1)
        {
            await AwardIfMissingAsync(userId, "first_quiz");
        }
    }

    public async Task EnsureTopicCompleteAchievementAsync(Guid userId, Guid topicId)
    {
        // Compare total lessons in topic with user's completed lessons in that topic.
        var totalLessonsInTopic = await _context.Lessons.CountAsync(l => l.TopicId == topicId);
        if (totalLessonsInTopic == 0)
        {
            return;
        }

        var completedLessonsInTopic = await (
            from progress in _context.UserLessonProgresses
            join lesson in _context.Lessons on progress.LessonId equals lesson.Id
            where progress.UserId == userId
                && progress.IsCompleted
                && lesson.TopicId == topicId
            select progress.LessonId)
            .Distinct()
            .CountAsync();

        if (completedLessonsInTopic == totalLessonsInTopic)
        {
            await AwardIfMissingAsync(userId, "topic_complete");
        }
    }

    private async Task AwardIfMissingAsync(Guid userId, string achievementCode)
    {
        // Look up achievement by code and skip when it does not exist.
        var achievement = await _context.Achievements
            .FirstOrDefaultAsync(a => a.Code == achievementCode);

        if (achievement is null)
        {
            return;
        }

        var alreadyEarned = await _context.UserAchievements
            .AnyAsync(x => x.UserId == userId && x.AchievementId == achievement.Id);

        if (alreadyEarned)
        {
            return;
        }

        // Create a user-achievement row only once.
        await _context.UserAchievements.AddAsync(new UserAchievement
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AchievementId = achievement.Id,
            EarnedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }
}
