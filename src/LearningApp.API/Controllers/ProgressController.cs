using System.Security.Claims;
using LearningApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProgressController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID claim missing or invalid.");
        }

        var totalLessons = await _context.Lessons.CountAsync();
        var completedLessons = await _context.UserLessonProgresses
            .CountAsync(p => p.UserId == userId && p.IsCompleted);

        var percentage = totalLessons == 0
            ? 0
            : (int)Math.Round((double)completedLessons / totalLessons * 100);

        return Ok(new
        {
            totalLessons,
            completedLessons,
            percentage
        });
    }

    [HttpGet("topic/{topicId:guid}")]
    public async Task<IActionResult> GetTopicProgress(Guid topicId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID claim missing or invalid.");
        }

        var topicExists = await _context.Topics.AnyAsync(t => t.Id == topicId);
        if (!topicExists)
        {
            return NotFound("Topic not found.");
        }

        var totalLessons = await _context.Lessons.CountAsync(l => l.TopicId == topicId);

        var completedLessons = await (
            from progress in _context.UserLessonProgresses
            join lesson in _context.Lessons on progress.LessonId equals lesson.Id
            where progress.UserId == userId
                && progress.IsCompleted
                && lesson.TopicId == topicId
            select progress.Id)
            .CountAsync();

        var percentage = totalLessons == 0
            ? 0
            : (int)Math.Round((double)completedLessons / totalLessons * 100);

        return Ok(new
        {
            topicId,
            totalLessons,
            completedLessons,
            percentage
        });
    }
}