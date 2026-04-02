using LearningApp.API.DTOs.Admin.Topics;
using LearningApp.Domain.Entities;
using LearningApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/topics")]
public class AdminTopicsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminTopicsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTopics()
    {
        var topics = await _context.Topics
            .OrderBy(t => t.Order)
            .Select(t => new AdminTopicResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Order = t.Order,
                LessonCount = t.Lessons.Count,
                QuizCount = t.Quizzes.Count
            })
            .ToListAsync();

        return Ok(topics);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTopicById(Guid id)
    {
        var topic = await _context.Topics
            .Where(t => t.Id == id)
            .Select(t => new AdminTopicResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Order = t.Order,
                LessonCount = t.Lessons.Count,
                QuizCount = t.Quizzes.Count
            })
            .FirstOrDefaultAsync();

        if (topic is null)
        {
            return NotFound("Topic not found.");
        }

        return Ok(topic);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTopic(AdminTopicCreateDto request)
    {
        var topic = new Topic
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Order = request.Order
        };

        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTopicById), new { id = topic.Id }, new AdminTopicResponseDto
        {
            Id = topic.Id,
            Title = topic.Title,
            Description = topic.Description,
            Order = topic.Order,
            LessonCount = 0,
            QuizCount = 0
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTopic(Guid id, AdminTopicUpdateDto request)
    {
        var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == id);
        if (topic is null)
        {
            return NotFound("Topic not found.");
        }

        topic.Title = request.Title;
        topic.Description = request.Description;
        topic.Order = request.Order;

        await _context.SaveChangesAsync();

        var response = new AdminTopicResponseDto
        {
            Id = topic.Id,
            Title = topic.Title,
            Description = topic.Description,
            Order = topic.Order,
            LessonCount = await _context.Lessons.CountAsync(l => l.TopicId == topic.Id),
            QuizCount = await _context.Quizzes.CountAsync(q => q.TopicId == topic.Id)
        };

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTopic(Guid id)
    {
        var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == id);
        if (topic is null)
        {
            return NotFound("Topic not found.");
        }

        var hasLessons = await _context.Lessons.AnyAsync(l => l.TopicId == id);
        var hasQuizzes = await _context.Quizzes.AnyAsync(q => q.TopicId == id);
        if (hasLessons || hasQuizzes)
        {
            return BadRequest("Topic cannot be deleted because it has related lessons or quizzes.");
        }

        _context.Topics.Remove(topic);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
