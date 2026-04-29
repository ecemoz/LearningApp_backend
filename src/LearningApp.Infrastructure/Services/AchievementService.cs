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

    public async Task EnsureTopicAchievementAsync(Guid userId, Guid topicId)
    {
        // Bulunulan topic ile ilişkilendirilmiş bir achievement var mı?
        var topicAchievement = await _context.Achievements
            .FirstOrDefaultAsync(a => a.TopicId == topicId);

        if (topicAchievement is null)
        {
            return; // Topic'e bağlı achievement yoksa çık
        }

        // Kullanıcı bu achievement'ı önceden kazanmış mı?
        var alreadyEarned = await _context.UserAchievements
            .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == topicAchievement.Id);

        if (alreadyEarned)
        {
            return;
        }

        // Topic'teki toplam lesson sayısı ve kullanıcının tamamladıkları
        var totalLessonsInTopic = await _context.Lessons.CountAsync(l => l.TopicId == topicId);
        
        var completedLessonsInTopic = await _context.UserLessonProgresses
            .Where(p => p.UserId == userId && p.IsCompleted && p.Lesson.TopicId == topicId)
            .Select(p => p.LessonId)
            .Distinct()
            .CountAsync();

        if (totalLessonsInTopic > 0 && completedLessonsInTopic < totalLessonsInTopic)
        {
            return; // Henüz tüm dersleri tamamlamamış
        }

        // Topic'teki quizler
        var totalQuizzesInTopic = await _context.Quizzes.CountAsync(q => q.TopicId == topicId);
        
        if (totalQuizzesInTopic > 0)
        {
            var attemptedQuizzesInTopic = await _context.UserQuizAttempts
                .Where(uqa => uqa.UserId == userId && uqa.Quiz.TopicId == topicId)
                .Select(uqa => uqa.QuizId)
                .Distinct()
                .CountAsync();

            if (attemptedQuizzesInTopic < totalQuizzesInTopic)
            {
                return; // Henüz tüm quizleri çözmemiş
            }
        }

        // Eğer lesson yoksa ve quiz yoksa edge case: 
        if (totalLessonsInTopic == 0 && totalQuizzesInTopic == 0)
        {
             return; // Boş topic için achievement vermeyelim
        }

        // Tüm dersler tamamlandı (veya yok), tüm quizler denendi (veya yok), achievement'ı ver!
        await _context.UserAchievements.AddAsync(new UserAchievement
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AchievementId = topicAchievement.Id,
            EarnedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
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
