using System.Text.RegularExpressions;


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

public class MutableString()
{
    public string Value { get; set; } = "";

}

public static class StringExtensions
{
    public static bool IsCompound(this string input)
    {
        return !string.IsNullOrWhiteSpace(input) && input.Trim().Contains(' ');
    }

    public static List<string> ToListOfStrings(this string input, string divisor = " ")
    {
        return input
            .Split(new[] { divisor }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }
    public static string[] ToArrayOfStrings(this string input, string divisor = " ")
    {
        if (string.IsNullOrWhiteSpace(input))
            return Array.Empty<string>();

        // Pattern matches:
        // - double quoted strings: "..."
        // - single quoted strings: '...'
        // - or unquoted sequences without the divisor
        var pattern = $"\"([^\"]*)\"|'([^']*)'|[^{Regex.Escape(divisor)}]+";

        return Regex.Matches(input, pattern)
            .Cast<Match>()
            .Select(m =>
                m.Groups[1].Success ? m.Groups[1].Value :
                m.Groups[2].Success ? m.Groups[2].Value :
                m.Value
            )
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
    }

    public static int RowCount(this string output)
    {
        if (string.IsNullOrEmpty(output))
            return 0;

        // Normalize newlines to \n for counting
        string normalized = output.Replace("\r\n", "\n").Replace("\r", "\n");

        // Split and count non-null lines
        return normalized.Split('\n').Length;
    }

    public static IEnumerable<string> SplitInParts(this string s, int partLength)
    {
        for (int i = 0; i < s.Length; i += partLength)
            yield return s.Substring(i, Math.Min(partLength, s.Length - i));
    }

    public static IEnumerable<string> SplitInParts(this string s, string separator, int minPartLength, int maxPartLength)
    {
        if (minPartLength > maxPartLength)
            throw new ArgumentException("minPartLength must be <= maxPartLength");

        int start = 0;
        while (start < s.Length)
        {
            int length = Math.Min(maxPartLength, s.Length - start);

            if (length < minPartLength)
            {
                // Last chunk, smaller than minPartLength, just yield and break
                yield return s.Substring(start, length);
                break;
            }

            // Find the next separator index after minPartLength within maxPartLength
            int searchStart = start + minPartLength;
            int searchEnd = start + length;

            int sepIndex = -1;
            if (searchStart < s.Length)
            {
                sepIndex = s.IndexOf(separator, searchStart, Math.Min(searchEnd - searchStart, s.Length - searchStart));
            }

            if (sepIndex != -1 && sepIndex <= searchEnd)
            {
                // Separator found between min and max, split here (include separator)
                int partLen = sepIndex - start + separator.Length;
                yield return s.Substring(start, partLen);
                start = sepIndex + separator.Length;
            }
            else
            {
                // No separator found in range, split forcibly at max length
                yield return s.Substring(start, length);
                start += length;
            }
        }
    }

    public static string ToMultilineString(this List<string> list)
    {
        return string.Join("\n", list);
    }

}

