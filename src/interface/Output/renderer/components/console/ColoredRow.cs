using Flexlib.Infrastructure.Environment;

namespace Flexlib.Interface.Output;

public static partial class Components
{
    public class ColoredRow
    {
        public string Text { get; set; }
        public ConsoleColor Color { get; set; } = ConsoleColor.White;

        public ColoredRow(string text, ConsoleColor color = ConsoleColor.White, bool truncate = false, int? maxWidth = null)
        {
            Text = truncate ? Truncate(text, maxWidth) : text;
            Color = color;
        }

        private static string Truncate(string text, int? width)
        {
            int max = width ?? Env.GetSafeWindowWidth();

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Length <= max
                ? text
                : text[..Math.Max(0, max - 1)] + "…";
        }
    }

    public static string ToMultiRowString(this List<ColoredRow> lines)
    {
        return string.Join(Environment.NewLine, lines.Select(l => l.Text));
    }
}
