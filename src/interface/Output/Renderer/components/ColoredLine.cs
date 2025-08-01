using System;
using System.Linq;
using System.Text;

namespace Flexlib.Interface.Output;

public static partial class Components
{
    public class ColoredLine
    {
        public string Text { get; set; }
        public ConsoleColor Color { get; set; } = ConsoleColor.White;

        public ColoredLine(string text, ConsoleColor color = ConsoleColor.White, bool truncate = true)
        {
            Text = truncate ? Truncate(text) : text;
            Color = color;
        }

        private static string Truncate(string text)
        {
            int width = Console.WindowWidth;
            return text.Length <= width ? text : text[..Math.Max(0, width - 1)] + "…";
        }
    }
}
