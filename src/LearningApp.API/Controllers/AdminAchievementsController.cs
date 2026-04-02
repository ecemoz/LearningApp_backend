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
            .OrderBy(a => a.Code)
            .Select(a => new AdminAchievementResponseDto
            {
                Id = a.Id,
                Code = a.Code,
                Title = a.Title,
                Description = a.Description
            })
            .ToListAsync();

        return Ok(achievements);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAchievementById(Guid id)
    {
        var achievement = await _context.Achievements
            .Where(a => a.Id == id)
            .Select(a => new AdminAchievementResponseDto
            {
                Id = a.Id,
                Code = a.Code,
                Title = a.Title,
                Description = a.Description
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
            Description = request.Description
        };

        _context.Achievements.Add(achievement);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAchievementById), new { id = achievement.Id }, new AdminAchievementResponseDto
        {
            Id = achievement.Id,
            Code = achievement.Code,
            Title = achievement.Title,
            Description = achievement.Description
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

        await _context.SaveChangesAsync();

        return Ok(new AdminAchievementResponseDto
        {
            Id = achievement.Id,
            Code = achievement.Code,
            Title = achievement.Title,
            Description = achievement.Description
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
