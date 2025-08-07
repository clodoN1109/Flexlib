namespace Flexlib.Infrastructure.Authentication;

public static class IdGenerator
{
    private static readonly char[] _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
    private static readonly Random _globalRandom = new();
    private static readonly object _lock = new();

    public static string GenerateRandomId(int length)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than 0.");

        var buffer = new char[length];

        lock (_lock) // Ensure thread safety
        {
            for (int i = 0; i < length; i++)
                buffer[i] = _chars[_globalRandom.Next(_chars.Length)];
        }

        return new string(buffer);
    }
}

