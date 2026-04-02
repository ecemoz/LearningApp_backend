using LearningApp.API.DTOs.Admin.Dashboard;
using LearningApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/dashboard")]
public class AdminDashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminDashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var response = new AdminDashboardSummaryResponseDto
        {
            TotalUsers = await _context.Users.CountAsync(),
            TotalTopics = await _context.Topics.CountAsync(),
            TotalLessons = await _context.Lessons.CountAsync(),
            TotalQuizzes = await _context.Quizzes.CountAsync(),
            TotalAchievements = await _context.Achievements.CountAsync()
        };

        return Ok(response);
    }
}
