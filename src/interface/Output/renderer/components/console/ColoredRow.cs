using Flexlib.Infrastructure.Environment;

namespace Flexlib.Interface.Output;

public static partial class Components
{
    public class ColoredRow
    {
        public string Text { get; set; }
        public ConsoleColor Color { get; set; } = ConsoleColor.White;

        public ColoredRow(string text, ConsoleColor color = ConsoleColor.White, bool truncate = true)
        {
            Text = truncate ? Truncate(text) : text;
            Color = color;
        }

        private static string Truncate(string text)
        {
            int width = Env.GetSafeWindowWidth();
            return text.Length <= width ? text : text[..Math.Max(0, width - 1)] + "…";
        }
    }

    public static string ToMultiRowString(this List<ColoredRow> lines)
    {
        return string.Join(Environment.NewLine, lines.Select(l => l.Text));
    }
}

