using LearningApp.Infrastructure.Persistence;
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
}