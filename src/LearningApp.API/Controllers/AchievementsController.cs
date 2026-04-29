using System.Security.Claims;
using LearningApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AchievementsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AchievementsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyAchievements()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID claim missing or invalid.");
        }

        var achievements = await _context.UserAchievements
            .Where(ua => ua.UserId == userId)
            .OrderByDescending(ua => ua.EarnedAt)
            .Select(ua => new
            {
                id = ua.AchievementId,
                code = ua.Achievement != null ? ua.Achievement.Code : string.Empty,
                title = ua.Achievement != null ? ua.Achievement.Title : string.Empty,
                description = ua.Achievement != null ? ua.Achievement.Description : string.Empty,
                topicId = ua.Achievement != null ? ua.Achievement.TopicId : null,
                topicTitle = ua.Achievement != null && ua.Achievement.Topic != null ? ua.Achievement.Topic.Title : null,
                earnedAt = ua.EarnedAt
            })
            .ToListAsync();

        return Ok(achievements);
    }
}
