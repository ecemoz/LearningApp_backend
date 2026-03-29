using LearningApp.API.DTOs.Auth;
using LearningApp.Domain.Entities;
using LearningApp.Infrastructure.Authentication;
using LearningApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LearningApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public AuthController(
        AppDbContext context,
        PasswordHasher passwordHasher,
        JwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto request)
    {
        var emailExists = await _context.Users.AnyAsync(x => x.Email == request.Email);
        if (emailExists)
        {
            return BadRequest("This email is already registered.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtTokenGenerator.GenerateToken(user);

        var response = new AuthResponseDto
        {
            Token = token,
            UserName = user.UserName,
            Email = user.Email
        };

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        if (user is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return Unauthorized("Invalid email or password.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        var response = new AuthResponseDto
        {
            Token = token,
            UserName = user.UserName,
            Email = user.Email
        };

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        return Ok(new
        {
            userId,
            userName,
            email
        });
    }
}