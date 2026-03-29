using System.Security.Cryptography;
using System.Text;

namespace LearningApp.Infrastructure.Authentication;

public class PasswordHasher
{
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var hashedInput = HashPassword(password);
        return hashedInput == passwordHash;
    }
}