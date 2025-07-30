using System;
using System.Linq;
using System.Text;
using Flexlib.Infrastructure.Environment;

namespace Flexlib.Interface.Output;

public static class Render
{
    public static string LineFilled(
        int totalWidth,
        string alignment = "left",
        char filler = '░',
        params string[] parts
    )
    {
        // Surround each part with a space
        var spacedParts = parts.Select(p => $" {p} ").ToArray();

        // Join them with the filler
        string content = string.Join(filler.ToString(), spacedParts);

        // Truncate if needed
        if (content.Length > totalWidth)
        {
            content = content.Substring(0, totalWidth);
        }
        else
        {
            int remaining = totalWidth - content.Length;

            if (alignment.ToLower() == "right")
            {
                content = new string(filler, remaining) + content;
            }
            else if (alignment.ToLower() == "center")
            {
                int padLeft = remaining / 2;
                int padRight = remaining - padLeft;
                content = new string(filler, padLeft) + content + new string(filler, padRight);
            }
            else // default to left
            {
                content = content + new string(filler, remaining);
            }
        }

        return content;
    }

    public static string LineFilled(int totalWidth, params string[] parts)
    {
        return LineFilled(totalWidth, "left", '░', parts);
    }

    public static string LineSpacedBetween(int totalWidth, params string[] parts)
    {
        if (parts == null || parts.Length == 0)
            return new string(' ', totalWidth);

        if (parts.Length == 1)
            return parts[0].PadRight(totalWidth);

        int contentWidth = parts.Sum(p => p.Length);
        int gaps = parts.Length - 1;
        int totalSpacing = Math.Max(0, totalWidth - contentWidth);
        int spacePerGap = totalSpacing / gaps;
        int extraSpaces = totalSpacing % gaps;

        var sb = new StringBuilder();

        for (int i = 0; i < parts.Length; i++)
        {
            sb.Append(parts[i]);

            if (i < gaps)
            {
                int spaceCount = spacePerGap + (extraSpaces-- > 0 ? 1 : 0);
                sb.Append(' ', spaceCount);
            }
        }

        return sb.ToString();
    }

    public static string Logo()
    {
        
        string logo = $">::>    flexlib";

        return logo;
        
    }

    public static string LogoLine(int totalWidth)
    {
        string idInfo;

#if DEBUG
        idInfo = $"{Env.BuildId}";
#else
        idInfo = $"v{Env.Version}";
#endif

        string logo = Render.Logo();

        string spaceBetween = new string(' ', totalWidth - logo.Length - idInfo.Length);
        
        string logoLine = logo + spaceBetween + idInfo;

        return logoLine;
        
    }

    public static List<string> WrapText(string text, int maxWidth)
    {
        var lines = new List<string>();
        var words = text.Split(' ');
        var currentLine = "";

        foreach (var word in words)
        {
            if ((currentLine + word).Length + 1 > maxWidth)
            {
                lines.Add(currentLine.TrimEnd());
                currentLine = "";
            }

            currentLine += word + " ";
        }

        if (!string.IsNullOrWhiteSpace(currentLine))
            lines.Add(currentLine.TrimEnd());

        return lines;
    }

}


