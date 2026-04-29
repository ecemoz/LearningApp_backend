using LearningApp.API.DTOs.Admin.Achievements;
using LearningApp.Domain.Entities;
using LearningApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/achievements")]
public class AdminAchievementsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminAchievementsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAchievements()
    {
        var achievements = await _context.Achievements
            .Include(a => a.Topic)
            .OrderBy(a => a.Code)
            .Select(a => new AdminAchievementResponseDto
            {
                Id = a.Id,
                Code = a.Code,
                Title = a.Title,
                Description = a.Description,
                TopicId = a.TopicId,
                TopicTitle = a.Topic != null ? a.Topic.Title : null
            })
            .ToListAsync();

        return Ok(achievements);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAchievementById(Guid id)
    {
        var achievement = await _context.Achievements
            .Include(a => a.Topic)
            .Where(a => a.Id == id)
            .Select(a => new AdminAchievementResponseDto
            {
                Id = a.Id,
                Code = a.Code,
                Title = a.Title,
                Description = a.Description,
                TopicId = a.TopicId,
                TopicTitle = a.Topic != null ? a.Topic.Title : null
            })
            .FirstOrDefaultAsync();

        if (achievement is null)
        {
            return NotFound("Achievement not found.");
        }

        return Ok(achievement);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAchievement(AdminAchievementCreateDto request)
    {
        var codeExists = await _context.Achievements.AnyAsync(a => a.Code == request.Code);
        if (codeExists)
        {
            return BadRequest("Achievement code must be unique.");
        }

        var achievement = new Achievement
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Title = request.Title,
            Description = request.Description,
            TopicId = request.TopicId
        };

        _context.Achievements.Add(achievement);
        await _context.SaveChangesAsync();

        var createdTopic = request.TopicId.HasValue 
            ? await _context.Topics.Where(t => t.Id == request.TopicId.Value).Select(t => t.Title).FirstOrDefaultAsync()
            : null;

        return CreatedAtAction(nameof(GetAchievementById), new { id = achievement.Id }, new AdminAchievementResponseDto
        {
            Id = achievement.Id,
            Code = achievement.Code,
            Title = achievement.Title,
            Description = achievement.Description,
            TopicId = achievement.TopicId,
            TopicTitle = createdTopic
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAchievement(Guid id, AdminAchievementUpdateDto request)
    {
        var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Id == id);
        if (achievement is null)
        {
            return NotFound("Achievement not found.");
        }

        var codeExists = await _context.Achievements
            .AnyAsync(a => a.Code == request.Code && a.Id != id);

        if (codeExists)
        {
            return BadRequest("Achievement code must be unique.");
        }

        achievement.Code = request.Code;
        achievement.Title = request.Title;
        achievement.Description = request.Description;
        achievement.TopicId = request.TopicId;

        await _context.SaveChangesAsync();

        var updatedTopic = request.TopicId.HasValue 
            ? await _context.Topics.Where(t => t.Id == request.TopicId.Value).Select(t => t.Title).FirstOrDefaultAsync()
            : null;

        return Ok(new AdminAchievementResponseDto
        {
            Id = achievement.Id,
            Code = achievement.Code,
            Title = achievement.Title,
            Description = achievement.Description,
            TopicId = achievement.TopicId,
            TopicTitle = updatedTopic
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAchievement(Guid id)
    {
        var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Id == id);
        if (achievement is null)
        {
            return NotFound("Achievement not found.");
        }

        var isEarned = await _context.UserAchievements.AnyAsync(ua => ua.AchievementId == id);
        if (isEarned)
        {
            return BadRequest("Achievement cannot be deleted because users have already earned it.");
        }

        _context.Achievements.Remove(achievement);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
