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
        string logoBar = Render.LogoLine(consoleWidth);
        string titleBar = "░░░░ COMMENTS " + new string('░', Math.Max(0, consoleWidth - 14));
        string bottomBar = new string('░', consoleWidth);

        const int padding = 3;
        const string ellipsis = "…";

        string Truncate(string text, int max) =>
            string.IsNullOrEmpty(text) ? "" : text.Length <= max ? text : text[..Math.Max(0, max - 1)] + ellipsis;

        if (comments == null)
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

    public List<ColoredLine> FormatItemTable(List<LibraryItem> items, Library lib, string filterSequence, string sortSequence, double localSizeInBytes, int consoleWidth)
    {
        var output = new List<ColoredLine>();

        string logoBar = Render.LogoLine(consoleWidth);
        string titleBar = "░░░░ LIBRARY ITEMS " + new string('░', Math.Max(0, consoleWidth - 20));
        string layoutSequence = string.Join("/", lib.LayoutSequence.Select(p => p.Name));
        string header = Render.LineFilled(consoleWidth, "left", ' ', $"{lib.Name}/{filterSequence}", $"{sortSequence}");

        string stats = $"{items.Count} items" + " " + $"{localSizeInBytes:N2} bytes";
        string footer = Render.LineSpacedBetween(consoleWidth, layoutSequence, stats);
        
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
        if ( rows.Count > 0 ) {
            for (int i = 0; i < columnCount; i++)
            {
                int maxDataWidth = rows.Max(r => r[i]?.Length ?? 0);
                idealColWidths[i] = Math.Max(headers[i].Length, maxDataWidth);
            }
        }
        else {
            for (int i = 0; i < columnCount; i++)
            {
                idealColWidths[i] = headers[i].Length;

            }
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

    public List<ColoredLine> FormatLibraryTable(List<Library> libraries, int consoleWidth)
    {
        var output = new List<ColoredLine>();

        string logoBar = Render.LogoLine(consoleWidth);
        string titleBar = "░░░░ LIBRARIES " + new string('░', Math.Max(0, consoleWidth - 15));
        string statsBar = Render.LineFilled(consoleWidth, "right", ' ', $"{libraries.Count} libraries");
        string bottomBar = new string('░', consoleWidth);

        const int padding = 3;
        const string ellipsis = "…";

        string TruncateEnd(string text, int max) =>
            string.IsNullOrEmpty(text) || text.Length <= max ? text : text[..Math.Max(0, max - 1)] + ellipsis;

        string TruncateStart(string text, int max) =>
            string.IsNullOrEmpty(text) || text.Length <= max ? text : ellipsis + text[^Math.Max(0, max - 1)..];

        var columns = new List<(string Header, Func<Library, string> ValueSelector, bool TruncateStart)>
        {
            ("Name", lib => lib.Name ?? "", false),
            ("Items", lib => lib.Items?.Count.ToString() ?? "0", false),
            ("Properties", lib => lib.PropertyDefinitions != null ? string.Join(", ", lib.PropertyDefinitions.Select(p => p.Name)) : "", false),
            ("Layout", lib => lib.LayoutSequence != null ? string.Join("/", lib.LayoutSequence.Select(l => l.Name)) : "", true),
            ("Location", lib => lib.Path ?? "", false)
        };

        int columnCount = columns.Count;

        // Get all row values as strings
        var rows = libraries.Select(lib => columns.Select(col => col.ValueSelector(lib)).ToArray()).ToList();

        int[] idealWidths;
        if (rows.Count > 0)
        {
            idealWidths = columns.Select((col, i) =>
                Math.Max(col.Header.Length, rows.Max(r => r[i]?.Length ?? 0))
            ).ToArray();
        }
        else
        {
            idealWidths = columns.Select(c => c.Header.Length).ToArray();
        }

        int totalPadding = (columnCount - 1) * padding;
        int totalIdeal = idealWidths.Sum() + totalPadding;

        int[] colWidths = new int[columnCount];
        Array.Copy(idealWidths, colWidths, columnCount);

        if (totalIdeal > consoleWidth)
        {
            int available = consoleWidth - totalPadding;

            // Try keeping the first three fixed
            int fixedCols = idealWidths.Take(3).Sum();

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
                for (int i = 0; i < 3; i++)
                    colWidths[i] = idealWidths[i];

                int rest = available - colWidths.Take(3).Sum();
                for (int i = 3; i < columnCount; i++)
                    colWidths[i] = Math.Min(rest, idealWidths[i]);
            }
        }

        // Draw table
        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(logoBar));
        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(titleBar, ConsoleColor.Gray));
        output.Add(new ColoredLine(""));

        output.Add(new ColoredLine(string.Join(" | ",
            columns.Select((col, i) => TruncateEnd(col.Header, colWidths[i]).PadRight(colWidths[i]))),
            ConsoleColor.DarkGray));

        output.Add(new ColoredLine(string.Join("-|-", colWidths.Select(w => new string('-', w))), ConsoleColor.DarkGray));

        foreach (var row in rows)
        {
            var formattedRow = row.Select((cell, i) =>
            {
                var width = colWidths[i];
                return (columns[i].TruncateStart ? TruncateStart(cell, width) : TruncateEnd(cell, width)).PadRight(width);
            });

            output.Add(new ColoredLine(string.Join(" | ", formattedRow)));
        }

        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(bottomBar, ConsoleColor.Gray));
        output.Add(new ColoredLine(statsBar, ConsoleColor.Gray));
        output.Add(new ColoredLine(""));

        return output;
    }
}


