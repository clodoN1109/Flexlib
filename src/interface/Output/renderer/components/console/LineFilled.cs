namespace Flexlib.Interface.Output;

public static partial class Components
{
    public static string LineFilled(
        int totalWidth,
        string alignment = "left",
        int padding = 0,
        char filler = '░',
        params string[] parts
    )
    {
        // Padding string around the whole content
        string paddingStr = new string(filler, padding);

        // Surround each part with a space
        var spacedParts = parts.Select(p => $" {p} ").ToArray();

        // Join them with the filler
        string content = string.Join(filler.ToString(), spacedParts);

        // Add padding to left & right of content
        content = paddingStr + content + paddingStr;

        // Truncate if needed
        if (content.Length > totalWidth)
        {
            return content.Substring(0, totalWidth);
        }

        int remaining = totalWidth - content.Length;

        switch (alignment.ToLower())
        {
            case "right":
                return new string(filler, remaining) + content;

            case "center":
                int padLeft = remaining / 2;
                int padRight = remaining - padLeft;
                return new string(filler, padLeft) + content + new string(filler, padRight);

            default: // left
                return content + new string(filler, remaining);
        }
    }

    public static string LineFilled(int totalWidth, params string[] parts)
    {
        return LineFilled(totalWidth, "left", 0, '░', parts);
    }
}
