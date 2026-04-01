using System.Security.Claims;
using LearningApp.Domain.Entities;
using LearningApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonsController : ControllerBase
{
    private readonly AppDbContext _context;

    public LessonsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLessonById(Guid id)
    {
        var lesson = await _context.Lessons
            .Where(l => l.Id == id)
            .Select(l => new
            {
                l.Id,
                l.TopicId,
                l.Title,
                l.Content,
                l.Order,
                l.IsLocked
            })
            .FirstOrDefaultAsync();

        if (lesson is null)
        {
            return NotFound("Lesson not found.");
        }

        return Ok(lesson);
    }

    [Authorize]
    [HttpPost("{lessonId:guid}/complete")]
    public async Task<IActionResult> CompleteLesson(Guid lessonId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID claim missing or invalid.");
        }

        var lessonExists = await _context.Lessons.AnyAsync(l => l.Id == lessonId);
        if (!lessonExists)
        {
            return NotFound("Lesson not found.");
        }

        var progress = await _context.UserLessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

        if (progress is not null)
        {
            if (progress.IsCompleted)
            {
                return BadRequest("Lesson already completed.");
            }

            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            _context.UserLessonProgresses.Add(new UserLessonProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = lessonId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            lessonId,
            completed = true
        });
    }
}