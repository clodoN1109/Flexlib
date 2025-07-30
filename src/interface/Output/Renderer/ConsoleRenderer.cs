using Flexlib.Domain;
using System.Text.Json;
using System.Drawing;
using Flexlib.Application.Ports;
using Flexlib.Interface.CLI;

namespace Flexlib.Interface.Output;

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

public class ConsoleRenderer
{
    
    public string Message(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "";

        int maxBoxWidth = Console.WindowWidth - 4; // Leave margin
        int innerWidth = Math.Max(10, maxBoxWidth - 6); // space for borders

        // Split message into initial lines (on \n) and then wrap each one
        var wrappedLines = new List<string>();

        foreach (var rawLine in message.Split('\n'))
        {
            var remaining = rawLine.Trim();
            while (remaining.Length > innerWidth)
            {
                wrappedLines.Add("   " + remaining[..innerWidth]);
                remaining = remaining[innerWidth..];
            }

            wrappedLines.Add("   " + remaining); // Final (or short) part
        }

        int contentWidth = wrappedLines.Max(l => l.Length);
        int boxWidth = contentWidth + 2;

        string top = "---" + new string('―', boxWidth - 3) + "┐";
        string bottom = "---" + new string('―', boxWidth - 3) + "┘";

        var box = new List<string> { top };

        foreach (var line in wrappedLines)
        {
            string padded = line.PadRight(contentWidth);
            box.Add($" {padded} │");
        }

        box.Add(bottom);
        return string.Join("\n", box);
    }

    public string UserInfo(IUser user) => Message($"user: {user.Id}");
    public string Success(string message) => Message($"✓ {message}");
    public string Failure(string message) => Message($"✗ {message}");
    public string Error(string message) => Message($"Error: {message}");

    public List<ColoredLine> AvailableActions(List<string> actions, int consoleWidth)
    {
        var lines = new List<ColoredLine>();

        if (actions == null || actions.Count == 0)
        {
            lines.Add(new ColoredLine("No available actions.", ConsoleColor.DarkGray));
            return lines;
        }

        const int padding = 2;
        string label = "commands: ";
        var commandLines = new List<string>();

        string currentLine = label;
        int currentWidth = label.Length;

        foreach (var cmd in actions)
        {
            string segment = cmd + new string(' ', padding);

            if (currentWidth + segment.Length > consoleWidth - 4) // account for border + margin
            {
                commandLines.Add(currentLine.TrimEnd());
                currentLine = new string(' ', label.Length) + segment;
                currentWidth = label.Length + segment.Length;
            }
            else
            {
                currentLine += segment;
                currentWidth += segment.Length;
            }
        }

        if (!string.IsNullOrWhiteSpace(currentLine))
        {
            commandLines.Add(currentLine.TrimEnd());
        }

        // Render boxed output
        string borderLine = "---" + new string('―', consoleWidth - 6) + "┐";
        lines.Add(new ColoredLine(borderLine, ConsoleColor.DarkGray));

        foreach (var line in commandLines)
        {
            string padded = "  " + line.PadRight(consoleWidth - 6);
            lines.Add(new ColoredLine(padded + " │", ConsoleColor.Gray));
        }

        string bottomLine = "---" + new string('―', consoleWidth - 6) + "┘";
        lines.Add(new ColoredLine(bottomLine, ConsoleColor.DarkGray));

        return lines;
    }

    public List<ColoredLine> UsageInfo(UsageInfo info, int consoleWidth)
    {
        var lines = new List<ColoredLine>();

        string separator = new string('░', consoleWidth);
        string logo = Render.LogoLine(consoleWidth);
        string title = $"░░░░ {info.Group.Icon} {info.Title.ToUpperInvariant()}" + " ";
        string paddedTitle = title + new string('░', Math.Max(0, consoleWidth - title.Length));

        lines.Add(new ColoredLine(""));
        lines.Add(new ColoredLine(logo));
        lines.Add(new ColoredLine(""));
        lines.Add(new ColoredLine(paddedTitle, ConsoleColor.Gray));

        // Metadata
        if (info.Meta?.Any() == true)
        {
            lines.Add(new ColoredLine(""));
            lines.Add(new ColoredLine(string.Join("  •  ", info.Meta), ConsoleColor.DarkGray));
        }

        // Description
        if (!string.IsNullOrWhiteSpace(info.Description))
        {
            lines.Add(new ColoredLine(""));
            var wrapped = Render.WrapText(info.Description, consoleWidth);
            foreach (var line in wrapped)
                lines.Add(new ColoredLine(line, ConsoleColor.White));
        }

        // Usage Syntax
        if (!string.IsNullOrWhiteSpace(info.Syntax))
        {
            lines.Add(new ColoredLine(""));
            lines.Add(new ColoredLine("usage:", ConsoleColor.Cyan));
            lines.Add(new ColoredLine(""));
            lines.Add(new ColoredLine("   " + info.Syntax, ConsoleColor.White));
        }

        // Options
        if (info.Options?.Any() == true)
        {
            lines.Add(new ColoredLine(""));
            lines.Add(new ColoredLine("options:", ConsoleColor.Cyan));
            lines.Add(new ColoredLine(""));

            foreach (var opt in info.Options.OrderBy(opt => opt.Name))
            {
                var name = opt.Mandatory ? $"<{opt.Name}>" : $"[{opt.Name}]";
                var domain = opt.OptionDomain?.IncludedValues?.Any() == true
                    ? $" ({string.Join("|", opt.OptionDomain.IncludedValues.OrderBy(v => v))})"
                    : "";
                var defaultVal = !string.IsNullOrWhiteSpace(opt.DefaultValue)
                    ? $" (default: {opt.DefaultValue})"
                    : "";

                var label = $"    {name}{domain}{defaultVal}";
                lines.Add(new ColoredLine(label, opt.Mandatory ? ConsoleColor.Yellow : ConsoleColor.DarkGray, false));

                var wrappedDesc = Render.WrapText(opt.Description, consoleWidth - 6);
                foreach (var descLine in wrappedDesc)
                    lines.Add(new ColoredLine("      " + descLine, ConsoleColor.Gray));
                
                lines.Add(new ColoredLine("      " + opt.Syntax, ConsoleColor.Gray));
            }
        }

        lines.Add(new ColoredLine(""));
        lines.Add(new ColoredLine(separator, ConsoleColor.Gray));
        lines.Add(new ColoredLine(""));

        return lines;
    }

    public List<ColoredLine> FormatCommentTable(List<Comment> comments, string itemName, string libName, int consoleWidth)
    {
        var output = new List<ColoredLine>();

        // Setup headers and metadata
        string logoBar   = Render.LogoLine(consoleWidth);
        string titleBar  = "░░░░ COMMENTS " + new string('░', Math.Max(0, consoleWidth - 14));
        string header    = Render.LineFilled(consoleWidth, "left", ' ', $"{libName}/{itemName}");
        string statsBar  = Render.LineFilled(consoleWidth, "right", ' ', $"{comments.Count} comments");
        string bottomBar = new string('░', consoleWidth);

        var headers = new[] { "ID", "Author", "Text", "Created at", "Edited at" };
        const int padding = 3;
        const string ellipsis = "…";

        // Build table rows
        var rows = comments
            .Where(c => c != null)
            .Select(c => new[]
            {
                c.Id ?? "",
                c.Author?.Name ?? "",
                (c.Text ?? "").Replace("\r", ""), // normalize to Unix-style newlines
                c.CreatedTime ?? "",
                c.EditedTime ?? ""
            })
            .ToList();

        // Truncation helper
        string Truncate(string text, int max) =>
            string.IsNullOrEmpty(text) ? "" : text.Length <= max ? text : text[..Math.Max(0, max - 1)] + ellipsis;

        // Compute ideal column widths
        int[] colWidths = new int[headers.Length];
        for (int i = 0; i < headers.Length; i++)
        {
            colWidths[i] = Math.Max(
                headers[i].Length,
                rows.Max(r => r[i].Split('\n').Max(line => line.Length))
            );
        }

        // Adjust widths to fit console
        int totalPadding = (headers.Length - 1) * padding;
        int totalWidth = colWidths.Sum() + totalPadding;

        if (totalWidth > consoleWidth)
        {
            int available = consoleWidth - totalPadding;
            double scale = (double)available / colWidths.Sum();

            for (int i = 0; i < colWidths.Length; i++)
                colWidths[i] = Math.Max(6, (int)Math.Floor(colWidths[i] * scale));

            int adjust = available - colWidths.Sum();
            for (int i = 0; adjust != 0 && i < colWidths.Length; i++)
            {
                colWidths[i] += Math.Sign(adjust);
                adjust -= Math.Sign(adjust);
            }
        }

        // Header rendering
        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(logoBar));
        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(titleBar, ConsoleColor.Gray));
        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(header));
        output.Add(new ColoredLine(""));

        // Table header
        var headerLine = string.Join(" | ",
            headers.Select((h, i) => Truncate(h, colWidths[i]).PadRight(colWidths[i])));
        output.Add(new ColoredLine(headerLine, ConsoleColor.DarkGray));
        output.Add(new ColoredLine(new string('-', consoleWidth), ConsoleColor.DarkGray));

        // Table body
        foreach (var row in rows)
        {
            // Split and align each cell into lines
            var splitCells = row.Select((cell, i) =>
                cell.Split('\n')
                    .Select(line => Truncate(line, colWidths[i]).PadRight(colWidths[i]))
                    .ToList()
            ).ToList();

            int maxLines = splitCells.Max(lines => lines.Count);

            // Ensure all columns have same number of lines
            for (int i = 0; i < splitCells.Count; i++)
            {
                while (splitCells[i].Count < maxLines)
                    splitCells[i].Add(new string(' ', colWidths[i]));
            }

            // Render each visual line
            for (int line = 0; line < maxLines; line++)
            {
                var formattedLine = string.Join(" | ",
                    splitCells.Select(col => col[line]));
                output.Add(new ColoredLine(formattedLine));
            }

            output.Add(new ColoredLine(new string('-', consoleWidth), ConsoleColor.DarkGray));
        }

        // Footer
        output.Add(new ColoredLine(""));
        output.Add(new ColoredLine(bottomBar, ConsoleColor.Gray));
        output.Add(new ColoredLine(statsBar));

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

        return output;
    }
}


