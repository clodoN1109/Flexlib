using System;
using System.Linq;
using System.Text;

namespace Flexlib.Interface.Output;

public static partial class Components
{
    public static List<string> WrappedText(string text, int maxWidth)
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

