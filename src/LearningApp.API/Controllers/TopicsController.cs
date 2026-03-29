using LearningApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TopicsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTopics()
    {
        var topics = await _context.Topics
            .OrderBy(t => t.Order)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.Order,
                LessonCount = t.Lessons.Count
            })
            .ToListAsync();

        return Ok(topics);
    }

[HttpGet("{topicId:guid}/lessons")]
public async Task<IActionResult> GetLessonsByTopic(Guid topicId)
{
    var topicExists = await _context.Topics.AnyAsync(t => t.Id == topicId);

    if (!topicExists)
    {
        return NotFound("Topic not found.");
    }

    var lessons = await _context.Lessons
        .Where(l => l.TopicId == topicId)
        .OrderBy(l => l.Order)
        .Select(l => new
        {
            l.Id,
            l.Title,
            l.Order,
            l.IsLocked
        })
        .ToListAsync();

    return Ok(lessons);
}
}