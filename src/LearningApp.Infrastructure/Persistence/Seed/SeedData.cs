using LearningApp.Domain.Entities;
using LearningApp.Infrastructure.Authentication;
using LearningApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LearningApp.Infrastructure.Persistence.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        var passwordHasher = new PasswordHasher();

        await EnsureAdminUserAsync(context, passwordHasher);

        await context.SaveChangesAsync();
    }

    private static async Task EnsureAdminUserAsync(AppDbContext context, PasswordHasher passwordHasher)
    {
        var adminExists = await context.Users.AnyAsync(u =>
            u.UserName == "admin" || u.Email == "admin@learningapp.com");

        if (adminExists)
        {
            return;
        }

        await context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@learningapp.com",
            PasswordHash = passwordHasher.HashPassword("Admin123!"),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        });
    }
}