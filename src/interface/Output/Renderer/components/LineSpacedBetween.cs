using System;
using System.Linq;
using System.Text;

namespace Flexlib.Interface.Output;

public static partial class Components
{   
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

}

