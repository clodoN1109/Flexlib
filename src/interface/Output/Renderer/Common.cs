using System;
using System.Linq;

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

    public static string Logo()
    {  
        string logo = ">::>    flexlib";

        return logo;
        
    }

}

