using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Erp.Web.Services;

public record AppUser(string Email, string FirstName, string LastName, string PasswordHash, string Salt);

/// <summary>
/// Thread-safe in-memory user store with PBKDF2 password hashing.
/// Replace with a database-backed store (e.g., ASP.NET Core Identity) in production.
/// </summary>
public class InMemoryUserStore
{
    private readonly ConcurrentDictionary<string, AppUser> _users = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryUserStore()
    {
        // Seed a demo user for convenience
        Register("demo@dynamix.com", "Demo", "User", "Password123!");
    }

    public bool Register(string email, string firstName, string lastName, string password)
    {
        var (hash, salt) = HashPassword(password);
        var user = new AppUser(email.ToLowerInvariant(), firstName, lastName, hash, salt);
        return _users.TryAdd(user.Email, user);
    }

    public AppUser? Validate(string email, string password)
    {
        if (!_users.TryGetValue(email.ToLowerInvariant(), out var user))
            return null;
        return VerifyPassword(password, user.PasswordHash, user.Salt) ? user : null;
    }

    private static (string hash, string salt) HashPassword(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password, saltBytes, 100_000, HashAlgorithmName.SHA256, 32);
        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    private static bool VerifyPassword(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password, saltBytes, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(hashBytes, Convert.FromBase64String(hash));
    }
}
