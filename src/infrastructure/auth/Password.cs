namespace Flexlib.Infrastructure.Authentication;

public static class Password
{
    public static bool Verify(string plainText, string hashed)
    {
        // Replace with actual secure verification, e.g., BCrypt or SHA256
        return Hash(plainText) == hashed;
    }

    public static string Hash(string plainText)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }
}

