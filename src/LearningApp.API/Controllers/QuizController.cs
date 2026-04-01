using System.Security.Claims;
using LearningApp.API.DTOs.Quiz;
using LearningApp.Domain.Entities;
using LearningApp.Infrastructure.Persistence;
using LearningApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.API.Controllers;

[ApiController]
[Route("api")]
public class QuizController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AchievementService _achievementService;

    public QuizController(AppDbContext context, AchievementService achievementService)
    {
        _context = context;
        _achievementService = achievementService;
    }

    [HttpGet("topics/{topicId:guid}/quiz")]
    public async Task<IActionResult> GetQuizByTopic(Guid topicId)
    {
        var topicExists = await _context.Topics.AnyAsync(t => t.Id == topicId);
        if (!topicExists)
        {
            return NotFound("Topic not found.");
        }

        // Project only fields that the client needs; do not return IsCorrect.
        var quiz = await _context.Quizzes
            .Where(q => q.TopicId == topicId)
            .Select(q => new QuizResponseDto
            {
                Id = q.Id,
                TopicId = q.TopicId,
                Title = q.Title,
                Questions = q.Questions
                    .Select(question => new QuizQuestionResponseDto
                    {
                        Id = question.Id,
                        QuestionText = question.QuestionText,
                        Options = question.Options
                            .Select(option => new QuizOptionResponseDto
                            {
                                Id = option.Id,
                                OptionText = option.OptionText
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (quiz is null)
        {
            return NotFound("Quiz not found for this topic.");
        }

        return Ok(quiz);
    }

    [Authorize]
    [HttpPost("quizzes/{quizId:guid}/submit")]
    public async Task<IActionResult> SubmitQuiz(Guid quizId, SubmitQuizRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID claim missing or invalid.");
        }

        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(question => question.Options)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz is null)
        {
            return NotFound("Quiz not found.");
        }

        var answers = request.Answers ?? new List<SubmitQuizAnswerDto>();
        var answersByQuestion = answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(group => group.Key, group => group.First().SelectedOptionId);

        // Count how many submitted answers match correct options.
        var correctCount = 0;

        foreach (var question in quiz.Questions)
        {
            if (!answersByQuestion.TryGetValue(question.Id, out var selectedOptionId))
            {
                continue;
            }

            var isCorrect = question.Options.Any(option => option.Id == selectedOptionId && option.IsCorrect);
            if (isCorrect)
            {
                correctCount++;
            }
        }

        var totalQuestionCount = quiz.Questions.Count;
        var score = totalQuestionCount == 0
            ? 0
            : correctCount * 100 / totalQuestionCount;

        var attempt = new UserQuizAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            QuizId = quiz.Id,
            Score = score,
            CorrectCount = correctCount,
            TotalQuestionCount = totalQuestionCount,
            AttemptedAt = DateTime.UtcNow
        };

        await _context.UserQuizAttempts.AddAsync(attempt);
        await _context.SaveChangesAsync();

        // Award first quiz achievement after a successful attempt save.
        await _achievementService.EnsureFirstQuizAchievementAsync(userId);

        var response = new QuizResultResponseDto
        {
            QuizId = quiz.Id,
            Score = score,
            CorrectCount = correctCount,
            TotalQuestionCount = totalQuestionCount,
            Message = score >= 80 ? "Great job!" : "Keep practicing!"
        };

        return Ok(response);
    }
}
