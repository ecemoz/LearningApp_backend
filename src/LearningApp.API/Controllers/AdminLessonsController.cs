using LearningApp.API.DTOs.Admin.Lessons;
using LearningApp.Domain.Entities;
using LearningApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/lessons")]
public class AdminLessonsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminLessonsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetLessons()
    {
        var lessons = await _context.Lessons
            .OrderBy(l => l.Order)
            .Select(l => new AdminLessonResponseDto
            {
                Id = l.Id,
                TopicId = l.TopicId,
                TopicTitle = l.Topic != null ? l.Topic.Title : string.Empty,
                Title = l.Title,
                Content = l.Content,
                Order = l.Order,
                IsLocked = l.IsLocked
            })
            .ToListAsync();

        return Ok(lessons);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLessonById(Guid id)
    {
        var lesson = await _context.Lessons
            .Where(l => l.Id == id)
            .Select(l => new AdminLessonResponseDto
            {
                Id = l.Id,
                TopicId = l.TopicId,
                TopicTitle = l.Topic != null ? l.Topic.Title : string.Empty,
                Title = l.Title,
                Content = l.Content,
                Order = l.Order,
                IsLocked = l.IsLocked
            })
            .FirstOrDefaultAsync();

        if (lesson is null)
        {
            return NotFound("Lesson not found.");
        }

        return Ok(lesson);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLesson(AdminLessonCreateDto request)
    {
        var topicExists = await _context.Topics.AnyAsync(t => t.Id == request.TopicId);
        if (!topicExists)
        {
            return BadRequest("TopicId is invalid. Topic not found.");
        }

        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            TopicId = request.TopicId,
            Title = request.Title,
            Content = request.Content,
            Order = request.Order,
            IsLocked = request.IsLocked
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();

        var topicTitle = await _context.Topics
            .Where(t => t.Id == lesson.TopicId)
            .Select(t => t.Title)
            .FirstAsync();

        return CreatedAtAction(nameof(GetLessonById), new { id = lesson.Id }, new AdminLessonResponseDto
        {
            Id = lesson.Id,
            TopicId = lesson.TopicId,
            TopicTitle = topicTitle,
            Title = lesson.Title,
            Content = lesson.Content,
            Order = lesson.Order,
            IsLocked = lesson.IsLocked
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateLesson(Guid id, AdminLessonUpdateDto request)
    {
        var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == id);
        if (lesson is null)
        {
            return NotFound("Lesson not found.");
        }

        var topicExists = await _context.Topics.AnyAsync(t => t.Id == request.TopicId);
        if (!topicExists)
        {
            return BadRequest("TopicId is invalid. Topic not found.");
        }

        lesson.TopicId = request.TopicId;
        lesson.Title = request.Title;
        lesson.Content = request.Content;
        lesson.Order = request.Order;
        lesson.IsLocked = request.IsLocked;

        await _context.SaveChangesAsync();

        var topicTitle = await _context.Topics
            .Where(t => t.Id == lesson.TopicId)
            .Select(t => t.Title)
            .FirstAsync();

        return Ok(new AdminLessonResponseDto
        {
            Id = lesson.Id,
            TopicId = lesson.TopicId,
            TopicTitle = topicTitle,
            Title = lesson.Title,
            Content = lesson.Content,
            Order = lesson.Order,
            IsLocked = lesson.IsLocked
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteLesson(Guid id)
    {
        var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == id);
        if (lesson is null)
        {
            return NotFound("Lesson not found.");
        }

        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
