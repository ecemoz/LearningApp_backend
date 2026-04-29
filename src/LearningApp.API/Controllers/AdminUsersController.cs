using System.Security.Claims;
using LearningApp.API.DTOs.Admin.Users;
using LearningApp.Domain.Entities;
using LearningApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminUsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .OrderBy(u => u.CreatedAt)
            .ThenBy(u => u.UserName)
            .ThenBy(u => u.Id)
            .Select(u => new AdminUserListItemResponseDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                CompletedLessonCount = u.LessonProgresses.Count(p => p.IsCompleted),
                QuizAttemptCount = u.QuizAttempts.Count,
                AchievementCount = u.UserAchievements.Count
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await GetUserResponseAsync(id);
        if (user is null)
        {
            return NotFound("User not found.");
        }

        return Ok(user);
    }

    [HttpGet("{id:guid}/progress")]
    public async Task<IActionResult> GetUserProgress(Guid id)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == id);
        if (!userExists)
        {
            return NotFound("User not found.");
        }

        var totalLessons = await _context.Lessons.CountAsync();
        var completedLessons = await _context.UserLessonProgresses.CountAsync(p => p.UserId == id && p.IsCompleted);

        var topicProgresses = await _context.Topics
            .OrderBy(t => t.Order)
            .ThenBy(t => t.Title)
            .Select(t => new AdminUserTopicProgressResponseDto
            {
                TopicId = t.Id,
                TopicTitle = t.Title,
                TotalLessons = t.Lessons.Count(),
                CompletedLessons = t.Lessons.Count(l => l.UserLessonProgresses.Any(p => p.UserId == id && p.IsCompleted))
            })
            .ToListAsync();

        foreach (var topicProgress in topicProgresses)
        {
            topicProgress.Percentage = topicProgress.TotalLessons == 0
                ? 0
                : (int)Math.Round((double)topicProgress.CompletedLessons / topicProgress.TotalLessons * 100);
        }

        var percentage = totalLessons == 0
            ? 0
            : (int)Math.Round((double)completedLessons / totalLessons * 100);

        var response = new AdminUserProgressResponseDto
        {
            TotalLessons = totalLessons,
            CompletedLessons = completedLessons,
            Percentage = percentage,
            TopicProgresses = topicProgresses
        };

        return Ok(response);
    }

    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, AdminUserRoleUpdateDto request)
    {
        var normalizedRole = request.Role.Trim();
        if (normalizedRole != "Admin" && normalizedRole != "User")
        {
            return BadRequest("Role must be either 'Admin' or 'User'.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound("User not found.");
        }

        user.Role = normalizedRole;
        await _context.SaveChangesAsync();

        var response = await GetUserResponseAsync(user.Id);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(adminIdClaim, out var adminId) && adminId == id)
        {
            return BadRequest("You cannot delete your own account.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound("User not found.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        await _context.UserLessonProgresses
            .Where(p => p.UserId == id)
            .ExecuteDeleteAsync();

        await _context.UserQuizAttempts
            .Where(a => a.UserId == id)
            .ExecuteDeleteAsync();

        await _context.UserAchievements
            .Where(a => a.UserId == id)
            .ExecuteDeleteAsync();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return NoContent();
    }

    private async Task<AdminUserResponseDto?> GetUserResponseAsync(Guid id)
    {
        return await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new AdminUserResponseDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                CompletedLessonCount = u.LessonProgresses.Count(p => p.IsCompleted),
                QuizAttemptCount = u.QuizAttempts.Count,
                AchievementCount = u.UserAchievements.Count
            })
            .FirstOrDefaultAsync();
    }
}