using Flexlib.Domain;
using System.Text.Json;
using System.Drawing;

namespace Flexlib.Interface.Output;

public class ColoredLine
{
    public string Text { get; set; }
    public ConsoleColor Color { get; set; } = ConsoleColor.White;

    public ColoredLine(string text, ConsoleColor color = ConsoleColor.White)
    {
        Text = Truncate(text);
        Color = color;
    }

    private static string Truncate(string text)
    {
        int width = Console.WindowWidth;
        return text.Length <= width ? text : text[..Math.Max(0, width - 1)] + "…";
    }
}

public class Renderer
{
    public string Message(string? message) => $"\n{message}\n";

    public string ExplainUsage(string? usageInstructions) =>
        usageInstructions != null
            ? $"\n{usageInstructions}\n"
            : "\n Usage: flexlib {command} [options] \n";

    public string Success(string message) => $"\n✔  {message}\n";
    public string Failure(string message) => $"\n✖  {message}\n";
    public string Error(string message) => $"\nError: {message}\n";

    public List<ColoredLine> FormatCommentTable(List<Comment> comments, int consoleWidth)
    {
        var output = new List<ColoredLine>();
        string logoBar = $"  {Render.Logo()}";
        string titleBar = "░░░░ COMMENTS " + new string('░', Math.Max(0, consoleWidth - 14));
        string bottomBar = new string('░', consoleWidth);

        const int padding = 3;
        const string ellipsis = "…";

        string Truncate(string text, int max) =>
            string.IsNullOrEmpty(text) ? "" : text.Length <= max ? text : text[..Math.Max(0, max - 1)] + ellipsis;

        if (comments == null || comments.Count == 0)
        {
            output.Add(new ColoredLine("\nNo comments found."));
            return output;
        }

        var headers = new[] { "ID", "Text" };
        var rows = comments
            .Where(c => c != null)
            .Select(c => new[] { c.Id ?? "", c.Text.TrimEnd('\r', '\n') ?? "" })
            .ToList();

        int idWidth = rows.Select(r => r[0].Length).Append(headers[0].Length).Max();
        int availableTextWidth = consoleWidth - idWidth - padding;
        int maxTextContentWidth = rows.Select(r => r[1].Length).Append(headers[1].Length).Max();
        int textWidth = Math.Min(availableTextWidth, maxTextContentWidth);
        textWidth = Math.Max(headers[1].Length, textWidth);

        if (idWidth + textWidth + padding > consoleWidth)
        {
            textWidth = consoleWidth - idWidth - padding;
            if (textWidth < 6)
            {
                textWidth = 6;
                idWidth = consoleWidth - textWidth - padding;
            }
        }

        var colWidths = new[] { idWidth, textWidth };

        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(logoBar));
        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(titleBar, ConsoleColor.Gray));
        output.Add(new ColoredLine(""));

        var headerLine = string.Join(" | ",
            headers.Select((h, i) => Truncate(h, colWidths[i]).PadRight(colWidths[i])));
        output.Add(new ColoredLine(headerLine, ConsoleColor.DarkGray));

        string dividerRow = new string('-', consoleWidth);
        output.Add(new ColoredLine(dividerRow, ConsoleColor.DarkGray));

        foreach (var row in rows)
        {
            var formattedRow = string.Join(" | ",
                row.Select((cell, i) => Truncate(cell, colWidths[i]).PadRight(colWidths[i])));
            output.Add(new ColoredLine(formattedRow));
            output.Add(new ColoredLine(dividerRow, ConsoleColor.DarkGray));
        }

        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(bottomBar, ConsoleColor.Gray));
        output.Add(new ColoredLine(""));

        return output;
    }

    public List<ColoredLine> FormatItemTable(List<LibraryItem> items, Library lib, string filterSequence, string sortSequence, int consoleWidth)
    {
        var output = new List<ColoredLine>();

        string logoBar = $"  {Render.Logo()}";
        string titleBar = "░░░░ LIBRARY ITEMS " + new string('░', Math.Max(0, consoleWidth - 20));
        string layoutSequence = string.Join("/", lib.LayoutSequence.Select(p => p.Name));
        string header = Render.LineFilled(consoleWidth, "left", ' ', $"{lib.Name}/{filterSequence}", $"{sortSequence}");
        string footer = Render.LineFilled(consoleWidth, "left", ' ', layoutSequence);
        string bottomBar = new string('░', consoleWidth);

        const int padding = 3;
        const string ellipsis = "…";

        string Truncate(string text, int max) =>
            string.IsNullOrEmpty(text) ? "" : text.Length <= max ? text : text[..Math.Max(0, max - 1)] + ellipsis;

        var allKeys = items
            .Where(i => i.PropertyValues != null)
            .SelectMany(i => i.PropertyValues.Keys)
            .Distinct()
            .OrderBy(k => k)
            .ToList();

        var headers = new[] { "Id", "Name" }.Concat(allKeys).ToList();
        int columnCount = headers.Count;

        var rows = new List<string[]>();

        foreach (var item in items)
        {
            var row = new string[columnCount];
            row[0] = item.Id.ToString() ?? "";
            row[1] = item.Name ?? "";

            var propertyDict = item.GetPropertyValuesAsListOfStrings();

            for (int i = 0; i < allKeys.Count; i++)
            {
                var key = allKeys[i];
                if (propertyDict.TryGetValue(key, out var valueList) && valueList is not null)
                    row[i + 2] = string.Join(", ", valueList);
                else
                    row[i + 2] = "";
            }

            rows.Add(row);
        }

        int[] idealColWidths = new int[columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            int maxDataWidth = rows.Max(r => r[i]?.Length ?? 0);
            idealColWidths[i] = Math.Max(headers[i].Length, maxDataWidth);
        }

        int totalPadding = (columnCount - 1) * padding;
        int idealTotalWidth = idealColWidths.Sum() + totalPadding;

        int[] colWidths = new int[columnCount];
        if (idealTotalWidth <= consoleWidth)
        {
            Array.Copy(idealColWidths, colWidths, columnCount);
        }
        else
        {
            int availableWidth = consoleWidth - totalPadding;
            double scale = (double)availableWidth / idealColWidths.Sum();

            for (int i = 0; i < columnCount; i++)
                colWidths[i] = Math.Max(6, (int)Math.Floor(idealColWidths[i] * scale));

            int diff = availableWidth - colWidths.Sum();
            for (int i = 0; diff != 0 && i < columnCount; i++)
            {
                int adjust = diff > 0 ? 1 : -1;
                colWidths[i] += adjust;
                diff -= adjust;
            }
        }

        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(logoBar));
        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(titleBar, ConsoleColor.Gray));
        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(header, ConsoleColor.DarkGray));
        output.Add(new ColoredLine(""));

        output.Add(new ColoredLine(string.Join(" | ",
            headers.Select((h, i) => Truncate(h, colWidths[i]).PadRight(colWidths[i]))), ConsoleColor.DarkGray));

        output.Add(new ColoredLine(string.Join("-|-", colWidths.Select(w => new string('-', w))), ConsoleColor.DarkGray));

        foreach (var row in rows)
        {
            output.Add(new ColoredLine(string.Join(" | ",
                row.Select((cell, i) => Truncate(cell, colWidths[i]).PadRight(colWidths[i])))));
        }

        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(bottomBar, ConsoleColor.Gray));
        output.Add(new ColoredLine(footer, ConsoleColor.DarkGray));
        output.Add(new ColoredLine(""));

        return output;
    }

    public List<ColoredLine> FormatLibraryTable(List<Library> items, int consoleWidth)
    {
        var output = new List<ColoredLine>();

        string logoBar = $"  {Render.Logo()}";
        string titleBar = "░░░░ LIBRARIES " + new string('░', Math.Max(0, consoleWidth - 15));
        string bottomBar = new string('░', consoleWidth);

        var headers = new[] { "Name", "Properties", "Layout", "Path" };
        int columnCount = headers.Length;
        const int padding = 3;
        const string ellipsis = "…";

        string TruncateEnd(string text, int max) =>
            string.IsNullOrEmpty(text) || text.Length <= max ? text : text[..Math.Max(0, max - 1)] + ellipsis;

        string TruncateStart(string text, int max) =>
            string.IsNullOrEmpty(text) || text.Length <= max ? text : ellipsis + text[^Math.Max(0, max - 1)..];

        var rows = items.Select(lib => new[]
        {
            lib.Name ?? "",
            lib.PropertyDefinitions != null ? string.Join(", ", lib.PropertyDefinitions.Select(p => p.Name)) : "",
            lib.LayoutSequence != null ? string.Join("/", lib.LayoutSequence.Select(l => l.Name)) : "",
            lib.Path ?? ""
        }).ToList();

        int[] idealWidths = headers.Select((h, i) =>
            Math.Max(h.Length, rows.Max(r => r[i]?.Length ?? 0))).ToArray();

        int totalPadding = (columnCount - 1) * padding;
        int totalIdeal = idealWidths.Sum() + totalPadding;

        int[] colWidths = new int[columnCount];
        Array.Copy(idealWidths, colWidths, columnCount);

        if (totalIdeal > consoleWidth)
        {
            int available = consoleWidth - totalPadding;
            int fixedCols = idealWidths[0] + idealWidths[1] + idealWidths[2];

            if (fixedCols + 10 > available)
            {
                double scale = (double)available / idealWidths.Sum();
                for (int i = 0; i < columnCount; i++)
                    colWidths[i] = Math.Max(6, (int)Math.Floor(idealWidths[i] * scale));

                int adjust = available - colWidths.Sum();
                for (int i = 0; adjust != 0 && i < columnCount; i++)
                {
                    colWidths[i] += Math.Sign(adjust);
                    adjust -= Math.Sign(adjust);
                }
            }
            else
            {
                colWidths[0] = idealWidths[0];
                colWidths[1] = idealWidths[1];
                colWidths[2] = idealWidths[2];
                colWidths[3] = available - (colWidths[0] + colWidths[1] + colWidths[2]);
            }
        }

        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(logoBar));
        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(titleBar, ConsoleColor.Gray));
        output.Add(new ColoredLine(""));

        output.Add(new ColoredLine(string.Join(" | ", headers.Select((h, i) => TruncateEnd(h, colWidths[i]).PadRight(colWidths[i]))), ConsoleColor.DarkGray));
        output.Add(new ColoredLine(string.Join("-|-", colWidths.Select(w => new string('-', w))), ConsoleColor.DarkGray));

        foreach (var row in rows)
        {
            var formattedRow = row.Select((cell, i) =>
            {
                var width = colWidths[i];
                return (i == 3 ? TruncateStart(cell, width) : TruncateEnd(cell, width)).PadRight(width);
            });

            output.Add(new ColoredLine(string.Join(" | ", formattedRow)));
        }

        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(bottomBar, ConsoleColor.Gray));
        output.Add(new ColoredLine(""));

        return output;
    }
}


