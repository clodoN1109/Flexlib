namespace Flexlib.Infrastructure.Processing;

public static class TextUtil
{
    public static string Truncate(string input, int maxLength = 40)
    {
        return input.Length <= maxLength ? input : input.Substring(0, maxLength);
    }

    public static List<string> ParseCommaSeparated(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<string>();

        return input
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }

    public static string CenterText(string text, int width)
    {
        int padding = Math.Max(0, (width - text.Length) / 2);
        return new string(' ', padding) + text;
    }

}

public static class StringExtensions
{
    public static bool IsCompound(this string input)
    {
        return !string.IsNullOrWhiteSpace(input) && input.Trim().Contains(' ');
    }
}

