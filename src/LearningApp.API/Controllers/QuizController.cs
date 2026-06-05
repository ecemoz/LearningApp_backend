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
        Console.WriteLine($"[GetQuizByTopic] Endpoint entered. topicId: \"{topicId}\"");
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

        var attemptCount = 0;
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            attemptCount = await _context.UserQuizAttempts.CountAsync(x => x.UserId == userId && x.QuizId == quiz.Id);
        }
        quiz.AttemptCount = attemptCount;

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
        var questionResults = new List<QuestionResultDto>();

        foreach (var question in quiz.Questions)
        {
            var correctOption = question.Options.FirstOrDefault(option => option.IsCorrect);
            var correctOptionId = correctOption?.Id ?? Guid.Empty;

            if (!answersByQuestion.TryGetValue(question.Id, out var selectedOptionId))
            {
                questionResults.Add(new QuestionResultDto
                {
                    QuestionId = question.Id,
                    IsCorrect = false,
                    CorrectOptionId = correctOptionId
                });
                continue;
            }

            var isCorrect = question.Options.Any(option => option.Id == selectedOptionId && option.IsCorrect);
            if (isCorrect)
            {
                correctCount++;
            }

            questionResults.Add(new QuestionResultDto
            {
                QuestionId = question.Id,
                IsCorrect = isCorrect,
                CorrectOptionId = correctOptionId
            });
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
        await _achievementService.EnsureTopicAchievementAsync(userId, quiz.TopicId);

        var attemptCount = await _context.UserQuizAttempts.CountAsync(x => x.UserId == userId && x.QuizId == quiz.Id);

        var response = new QuizResultResponseDto
        {
            QuizId = quiz.Id,
            Score = score,
            CorrectCount = correctCount,
            TotalQuestionCount = totalQuestionCount,
            AttemptCount = attemptCount,
            Message = score >= 80 ? "Great job!" : "Keep practicing!",
            QuestionResults = questionResults
        };

        return Ok(response);
    }

    [Authorize]
    [HttpPost("quiz/explain-question")]
    public async Task<IActionResult> ExplainQuestion([FromBody] ExplainQuestionRequestDto request)
    {
        if (request == null)
        {
            return BadRequest("Request cannot be null.");
        }

        Console.WriteLine($"[ExplainQuestion] Endpoint entered. QuestionId: \"{request.QuestionId}\", QuestionText: \"{request.QuestionText}\", SelectedOptionText: \"{request.SelectedOptionText}\"");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID claim missing or invalid.");
        }


        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("[ExplainQuestion] GEMINI_API_KEY env variable is null or empty. Using fairytale fallback response.");
            return Ok(new { explanation = $"Sevgili öğrencim, seçtiğin şık seni geçici bir illüzyonun içine çekmiş. '{request?.SelectedOptionText}' cevabı ilk bakışta cazip gelse de, sorudaki asıl tılsımı gözden kaçırmana neden olmuş. Doğru cevaba ulaşmak için soruda gizlenmiş ipuçlarını sakin bir zihinle tekrar incele; bir sonraki denemende doğru kapıyı açacağına eminim!" });
        }

        try
        {
            using var client = new System.Net.Http.HttpClient();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";
            
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"Soru: {request?.QuestionText}\nSeçilen Yanlış Şık: {request?.SelectedOptionText}" }
                        }
                    }
                },
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new { text = "Sen eğlenceli bir eğitmensin. Öğrenciye şu soruda neden hata yaptığını ve doğrusunu nasıl bulacağını, masalsı temamıza uygun, çok kısa (en fazla 2 cümle) ve motive edici bir dille açıkla." }
                    }
                }
            };

            var jsonContent = System.Text.Json.JsonSerializer.Serialize(payload);
            Console.WriteLine("[ExplainQuestion] Sending request to Gemini API...");
            var response = await client.PostAsync(url, new System.Net.Http.StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json"));
            
            Console.WriteLine($"[ExplainQuestion] Gemini API responded with status code: {response.StatusCode}");
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(responseBody);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                Console.WriteLine("[ExplainQuestion] Explanation successfully generated by Gemini API.");
                return Ok(new { explanation = text?.Trim() ?? string.Empty });
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ExplainQuestion] Gemini API error payload: {errorResponse}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExplainQuestion] Exception caught inside endpoint: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine("[ExplainQuestion] Returning fairytale fallback response due to API call failure or exception.");
        return Ok(new { explanation = $"Sevgili öğrencim, seçtiğin şık seni geçici bir illüzyonun içine çekmiş. '{request?.SelectedOptionText}' cevabı ilk bakışta cazip gelse de, sorudaki asıl tılsımı gözden kaçırmana neden olmuş. Doğru cevaba ulaşmak için soruda gizlenmiş ipuçlarını sakin bir zihinle tekrar incele; bir sonraki denemende doğru kapıyı açacağına eminim!" });
    }
}

public class ExplainQuestionRequestDto
{
    public Guid QuizId { get; set; }
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string SelectedOptionText { get; set; } = string.Empty;
}
