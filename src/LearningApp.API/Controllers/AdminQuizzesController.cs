using LearningApp.API.DTOs.Admin.Quizzes;
using LearningApp.Domain.Entities;
using LearningApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/quizzes")]
public class AdminQuizzesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminQuizzesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetQuizzes()
    {
        var quizzes = await _context.Quizzes
            .OrderBy(q => q.Title)
            .Select(q => new AdminQuizListItemResponseDto
            {
                Id = q.Id,
                TopicId = q.TopicId,
                TopicTitle = q.Topic != null ? q.Topic.Title : string.Empty,
                Title = q.Title,
                QuestionCount = q.Questions.Count
            })
            .ToListAsync();

        return Ok(quizzes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetQuizById(Guid id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Topic)
            .Include(q => q.Questions)
            .ThenInclude(question => question.Options)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz is null)
        {
            return NotFound("Quiz not found.");
        }

        return Ok(MapQuizDetail(quiz));
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuiz(AdminQuizCreateDto request)
    {
        var topicExists = await _context.Topics.AnyAsync(t => t.Id == request.TopicId);
        if (!topicExists)
        {
            return BadRequest("TopicId is invalid. Topic not found.");
        }

        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            TopicId = request.TopicId,
            Title = request.Title
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        var createdQuiz = await _context.Quizzes
            .Include(q => q.Topic)
            .Include(q => q.Questions)
            .ThenInclude(question => question.Options)
            .FirstAsync(q => q.Id == quiz.Id);

        return CreatedAtAction(nameof(GetQuizById), new { id = quiz.Id }, MapQuizDetail(createdQuiz));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateQuiz(Guid id, AdminQuizUpdateDto request)
    {
        var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == id);
        if (quiz is null)
        {
            return NotFound("Quiz not found.");
        }

        var topicExists = await _context.Topics.AnyAsync(t => t.Id == request.TopicId);
        if (!topicExists)
        {
            return BadRequest("TopicId is invalid. Topic not found.");
        }

        quiz.TopicId = request.TopicId;
        quiz.Title = request.Title;

        await _context.SaveChangesAsync();

        var updatedQuiz = await _context.Quizzes
            .Include(q => q.Topic)
            .Include(q => q.Questions)
            .ThenInclude(question => question.Options)
            .FirstAsync(q => q.Id == id);

        return Ok(MapQuizDetail(updatedQuiz));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteQuiz(Guid id)
    {
        var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == id);
        if (quiz is null)
        {
            return NotFound("Quiz not found.");
        }

        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{quizId:guid}/questions")]
    public async Task<IActionResult> AddQuestion(Guid quizId, AdminQuizQuestionCreateDto request)
    {
        var quizExists = await _context.Quizzes.AnyAsync(q => q.Id == quizId);
        if (!quizExists)
        {
            return NotFound("Quiz not found.");
        }

        var question = new QuizQuestion
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            QuestionText = request.QuestionText
        };

        _context.QuizQuestions.Add(question);
        await _context.SaveChangesAsync();

        return Ok(new AdminQuizQuestionResponseDto
        {
            Id = question.Id,
            QuizId = question.QuizId,
            QuestionText = question.QuestionText,
            Options = new List<AdminQuizOptionResponseDto>()
        });
    }

    [HttpPut("/api/admin/questions/{questionId:guid}")]
    public async Task<IActionResult> UpdateQuestion(Guid questionId, AdminQuizQuestionUpdateDto request)
    {
        var question = await _context.QuizQuestions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question is null)
        {
            return NotFound("Question not found.");
        }

        question.QuestionText = request.QuestionText;
        await _context.SaveChangesAsync();

        return Ok(new AdminQuizQuestionResponseDto
        {
            Id = question.Id,
            QuizId = question.QuizId,
            QuestionText = question.QuestionText,
            Options = question.Options
                .Select(o => new AdminQuizOptionResponseDto
                {
                    Id = o.Id,
                    QuizQuestionId = o.QuizQuestionId,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect
                })
                .ToList()
        });
    }

    [HttpDelete("/api/admin/questions/{questionId:guid}")]
    public async Task<IActionResult> DeleteQuestion(Guid questionId)
    {
        var question = await _context.QuizQuestions.FirstOrDefaultAsync(q => q.Id == questionId);
        if (question is null)
        {
            return NotFound("Question not found.");
        }

        _context.QuizQuestions.Remove(question);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("/api/admin/questions/{questionId:guid}/options")]
    public async Task<IActionResult> AddOption(Guid questionId, AdminQuizOptionCreateDto request)
    {
        var questionExists = await _context.QuizQuestions.AnyAsync(q => q.Id == questionId);
        if (!questionExists)
        {
            return NotFound("Question not found.");
        }

        var option = new QuizOption
        {
            Id = Guid.NewGuid(),
            QuizQuestionId = questionId,
            OptionText = request.OptionText,
            IsCorrect = request.IsCorrect
        };

        _context.QuizOptions.Add(option);

        if (request.IsCorrect)
        {
            // Keep only one correct option per question for MVP.
            var otherOptions = await _context.QuizOptions
                .Where(o => o.QuizQuestionId == questionId && o.Id != option.Id)
                .ToListAsync();

            foreach (var otherOption in otherOptions)
            {
                otherOption.IsCorrect = false;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new AdminQuizOptionResponseDto
        {
            Id = option.Id,
            QuizQuestionId = option.QuizQuestionId,
            OptionText = option.OptionText,
            IsCorrect = option.IsCorrect
        });
    }

    [HttpPut("/api/admin/options/{optionId:guid}")]
    public async Task<IActionResult> UpdateOption(Guid optionId, AdminQuizOptionUpdateDto request)
    {
        var option = await _context.QuizOptions.FirstOrDefaultAsync(o => o.Id == optionId);
        if (option is null)
        {
            return NotFound("Option not found.");
        }

        option.OptionText = request.OptionText;
        option.IsCorrect = request.IsCorrect;

        if (request.IsCorrect)
        {
            // Keep only one correct option per question for MVP.
            var otherOptions = await _context.QuizOptions
                .Where(o => o.QuizQuestionId == option.QuizQuestionId && o.Id != option.Id)
                .ToListAsync();

            foreach (var otherOption in otherOptions)
            {
                otherOption.IsCorrect = false;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new AdminQuizOptionResponseDto
        {
            Id = option.Id,
            QuizQuestionId = option.QuizQuestionId,
            OptionText = option.OptionText,
            IsCorrect = option.IsCorrect
        });
    }

    [HttpDelete("/api/admin/options/{optionId:guid}")]
    public async Task<IActionResult> DeleteOption(Guid optionId)
    {
        var option = await _context.QuizOptions.FirstOrDefaultAsync(o => o.Id == optionId);
        if (option is null)
        {
            return NotFound("Option not found.");
        }

        _context.QuizOptions.Remove(option);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static AdminQuizResponseDto MapQuizDetail(Quiz quiz)
    {
        return new AdminQuizResponseDto
        {
            Id = quiz.Id,
            TopicId = quiz.TopicId,
            TopicTitle = quiz.Topic != null ? quiz.Topic.Title : string.Empty,
            Title = quiz.Title,
            Questions = quiz.Questions
                .OrderBy(q => q.QuestionText)
                .Select(question => new AdminQuizQuestionResponseDto
                {
                    Id = question.Id,
                    QuizId = question.QuizId,
                    QuestionText = question.QuestionText,
                    Options = question.Options
                        .Select(option => new AdminQuizOptionResponseDto
                        {
                            Id = option.Id,
                            QuizQuestionId = option.QuizQuestionId,
                            OptionText = option.OptionText,
                            IsCorrect = option.IsCorrect
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
