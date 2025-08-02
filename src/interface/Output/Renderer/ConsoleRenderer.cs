using Flexlib.Domain;
using System.Text.Json;
using System.Drawing;
using Flexlib.Application.Ports;
using Flexlib.Interface.CLI;
using Flexlib.Infrastructure.Interop;
using Flexlib.Interface.Output;
using Flexlib.Interface.Input;

namespace Flexlib.Interface.Output;


public class ConsoleRenderer
{

    public Components.WrappedMessage Message(string? message, ConsoleColor color = ConsoleColor.Gray)
    {
        var lines = new List<Components.ColoredLine>();

        if (string.IsNullOrWhiteSpace(message))
            return new Components.WrappedMessage { Lines = lines };

        int maxBoxWidth = Console.WindowWidth - 4;
        int innerWidth = Math.Max(10, maxBoxWidth - 6);

        var wrappedLines = new List<string>();

        foreach (var rawLine in message.Split('\n'))
        {
            var remaining = rawLine.Trim();
            while (remaining.Length > innerWidth)
            {
                wrappedLines.Add("   " + remaining[..innerWidth]);
                remaining = remaining[innerWidth..];
            }

            wrappedLines.Add("   " + remaining);
        }

        int contentWidth = wrappedLines.Max(l => l.Length);
        int boxWidth = contentWidth + 2;

        string top = "---" + new string('―', boxWidth - 3) + "┐";
        string bottom = "---" + new string('―', boxWidth - 3) + "┘";

        lines.Add(new Components.ColoredLine(top, color));

        foreach (var line in wrappedLines)
        {
            string padded = line.PadRight(contentWidth);
            lines.Add(new Components.ColoredLine($" {padded} │", color));
        }

        lines.Add(new Components.ColoredLine(bottom, color));

        return new Components.WrappedMessage { Lines = lines };
    }

    public Components.WrappedMessage Success(string message) =>
        Message($"✓ {message}", ConsoleColor.Green);

    public Components.WrappedMessage Warning(string message) =>
        Message($"⚠  {message}", ConsoleColor.Yellow);

    public Components.WrappedMessage Failure(string message) =>
        Message($"✗ {message}", ConsoleColor.Red);

    public Components.WrappedMessage RenderResult(Result result)
    {
        var all = new List<Components.ColoredLine>();

        if (result.IsSuccess)
            all.AddRange(Success(result.SuccessMessage ?? "").Lines);

        if (result.IsWarning)
            all.AddRange(Warning(result.WarningMessage ?? "").Lines);

        if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            all.AddRange(Failure(result.ErrorMessage).Lines);

        return new Components.WrappedMessage { Lines = all };
    }

    public Components.WrappedMessage UserInfo(string userInfo) =>
        Message($"{userInfo} 🪪", ConsoleColor.Cyan);

    public Components.WrappedMessage AuthStatus(string message) =>
        Message($"🪪 {message}", ConsoleColor.Cyan);

    public Components.WrappedMessage Error(string message) =>
        Message($"Error: {message}", ConsoleColor.Red);

    public List<Components.ColoredLine> AvailableActions(List<string> actions, int consoleWidth)
    {
        var lines = new List<Components.ColoredLine>();

        if (actions == null || actions.Count == 0)
        {
            lines.Add(new Components.ColoredLine("No available actions.", ConsoleColor.DarkGray));
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
        lines.Add(new Components.ColoredLine(borderLine, ConsoleColor.DarkGray));

        foreach (var line in commandLines)
        {
            string padded = "  " + line.PadRight(consoleWidth - 6);
            lines.Add(new Components.ColoredLine(padded + " │", ConsoleColor.Gray));
        }

        string bottomLine = "---" + new string('―', consoleWidth - 6) + "┘";
        lines.Add(new Components.ColoredLine(bottomLine, ConsoleColor.DarkGray));

        return lines;
    }

    public List<Components.ColoredLine> UsageInfo(UsageInfo info, int consoleWidth)
    {
        var lines = new List<Components.ColoredLine>();

        string separator = new string('░', consoleWidth);
        string logo = Components.LogoLine(consoleWidth);
        string title = $"░░░░ {info.Group.Icon} {info.Title.ToUpperInvariant()}" + " ";
        string paddedTitle = title + new string('░', Math.Max(0, consoleWidth - title.Length));

        lines.Add(new Components.ColoredLine(""));
        lines.Add(new Components.ColoredLine(logo));
        lines.Add(new Components.ColoredLine(""));
        lines.Add(new Components.ColoredLine(paddedTitle, ConsoleColor.Gray));

        // Metadata
        if (info.Meta?.Any() == true)
        {
            lines.Add(new Components.ColoredLine(""));
            lines.Add(new Components.ColoredLine(string.Join("  •  ", info.Meta), ConsoleColor.DarkGray));
        }

        // Description
        if (!string.IsNullOrWhiteSpace(info.Description))
        {
            lines.Add(new Components.ColoredLine(""));
            var wrapped = Components.WrappedText(info.Description, consoleWidth);
            foreach (var line in wrapped)
                lines.Add(new Components.ColoredLine(line, ConsoleColor.White));
        }

        // Usage Syntax
        if (!string.IsNullOrWhiteSpace(info.Syntax))
        {
            lines.Add(new Components.ColoredLine(""));
            lines.Add(new Components.ColoredLine("usage:", ConsoleColor.Cyan));
            lines.Add(new Components.ColoredLine(""));
            lines.Add(new Components.ColoredLine("   " + info.Syntax, ConsoleColor.White));
        }

        // Options
        if (info.Options?.Any() == true)
        {
            lines.Add(new Components.ColoredLine(""));
            lines.Add(new Components.ColoredLine("options:", ConsoleColor.Cyan));
            lines.Add(new Components.ColoredLine(""));

            foreach (var opt in info.Options.OrderByDescending(opt => opt.Mandatory))
            {
                var name = opt.Mandatory ? $"<{opt.Name}>" : $"[{opt.Name}]";
                var domain = opt.OptionDomain?.IncludedValues?.Any() == true
                    ? $" ({string.Join("|", opt.OptionDomain.IncludedValues.OrderBy(v => v))})"
                    : "";
                var defaultVal = !string.IsNullOrWhiteSpace(opt.DefaultValue)
                    ? $" (default: {opt.DefaultValue})"
                    : "";

                var label = $"    {name}{domain}{defaultVal}";
                lines.Add(new Components.ColoredLine(label, opt.Mandatory ? ConsoleColor.Yellow : ConsoleColor.DarkGray, false));

                var wrappedDesc = Components.WrappedText(opt.Description, consoleWidth - 6);
                foreach (var descLine in wrappedDesc)
                    lines.Add(new Components.ColoredLine("      " + descLine, ConsoleColor.Gray));
                
                lines.Add(new Components.ColoredLine("      " + opt.Syntax, ConsoleColor.Gray));
            }
        }

        // Examples
        if (info.Examples.Count > 0 ) 
        {
            lines.Add(new Components.ColoredLine("examples:", ConsoleColor.Cyan));
        }
        foreach (var example in info.Examples){
            if (!string.IsNullOrWhiteSpace(example))
            {
                lines.Add(new Components.ColoredLine(""));
                lines.Add(new Components.ColoredLine("   " + example, ConsoleColor.White));
            }
        }

        lines.Add(new Components.ColoredLine(""));
        lines.Add(new Components.ColoredLine(separator, ConsoleColor.Gray));
        lines.Add(new Components.ColoredLine(""));

        return lines;
    }

    public List<Components.ColoredLine> FormatCommentTable(List<Comment> comments, string itemName, string libName, int consoleWidth)
    {
        var output = new List<Components.ColoredLine>();

        // Setup headers and metadata
        string logoBar   = Components.LogoLine(consoleWidth);
        string titleBar  = "░░░░ COMMENTS " + new string('░', Math.Max(0, consoleWidth - 14));
        string header    = Components.LineFilled(consoleWidth, "left", ' ', $"{libName}/{itemName}");
        string statsBar  = Components.LineFilled(consoleWidth, "right", ' ', $"{comments.Count} comments");
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
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(logoBar));
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(titleBar, ConsoleColor.Gray));
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(header));
        output.Add(new Components.ColoredLine(""));

        // Table header
        var headerLine = string.Join(" | ",
            headers.Select((h, i) => Truncate(h, colWidths[i]).PadRight(colWidths[i])));
        output.Add(new Components.ColoredLine(headerLine, ConsoleColor.DarkGray));
        output.Add(new Components.ColoredLine(new string('-', consoleWidth), ConsoleColor.DarkGray));

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
                output.Add(new Components.ColoredLine(formattedLine));
            }

            output.Add(new Components.ColoredLine(new string('-', consoleWidth), ConsoleColor.DarkGray));
        }

        // Footer
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(bottomBar, ConsoleColor.Gray));
        output.Add(new Components.ColoredLine(statsBar));

        return output;
    }

    public List<Components.ColoredLine> FormatItemTable(List<LibraryItem> items, Library lib, string filterSequence, string sortSequence, double localSizeInBytes, List<string> itemNameFilter, int consoleWidth)
    {
        var output = new List<Components.ColoredLine>();

        string logoBar = Components.LogoLine(consoleWidth);
        string titleBar = "░░░░ LIBRARY ITEMS " + new string('░', Math.Max(0, consoleWidth - 20));
        string layoutSequence = string.Join("/", lib.LayoutSequence.Select(p => p.Name));
        string header = Components.LineFilled(consoleWidth, "left", ' ', $"{lib.Name}/{filterSequence}/{string.Join('|', itemNameFilter)}", $"{sortSequence}");

        string stats = $"{items.Count} items" + " " + $"{localSizeInBytes:N2} bytes";
        string footer = Components.LineSpacedBetween(consoleWidth, layoutSequence, stats);
        
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

        var headers = new[] { "ID", "name" }.Concat(allKeys).ToList();
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

        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(logoBar));
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(titleBar, ConsoleColor.Gray));
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(header, ConsoleColor.DarkGray));
        output.Add(new Components.ColoredLine(""));

        output.Add(new Components.ColoredLine(string.Join(" | ",
            headers.Select((h, i) => Truncate(h, colWidths[i]).PadRight(colWidths[i]))), ConsoleColor.DarkGray));

        output.Add(new Components.ColoredLine(string.Join("-|-", colWidths.Select(w => new string('-', w))), ConsoleColor.DarkGray));

        foreach (var row in rows)
        {
            output.Add(new Components.ColoredLine(string.Join(" | ",
                row.Select((cell, i) => Truncate(cell, colWidths[i]).PadRight(colWidths[i])))));
        }

        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(bottomBar, ConsoleColor.Gray));
        output.Add(new Components.ColoredLine(footer, ConsoleColor.DarkGray));

        return output;
    }

    public List<Components.ColoredLine> FormatLibraryTable(List<Library> libraries, int consoleWidth)
    {
        var output = new List<Components.ColoredLine>();

        string logoBar = Components.LogoLine(consoleWidth);
        string titleBar = "░░░░ LIBRARIES " + new string('░', Math.Max(0, consoleWidth - 15));
        string statsBar = Components.LineFilled(consoleWidth, "right", ' ', $"{libraries.Count} libraries");
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
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(logoBar));
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(titleBar, ConsoleColor.Gray));
        output.Add(new Components.ColoredLine(""));

        output.Add(new Components.ColoredLine(string.Join(" | ",
            columns.Select((col, i) => TruncateEnd(col.Header, colWidths[i]).PadRight(colWidths[i]))),
            ConsoleColor.DarkGray));

        output.Add(new Components.ColoredLine(string.Join("-|-", colWidths.Select(w => new string('-', w))), ConsoleColor.DarkGray));

        foreach (var row in rows)
        {
            var formattedRow = row.Select((cell, i) =>
            {
                var width = colWidths[i];
                return (columns[i].TruncateStart ? TruncateStart(cell, width) : TruncateEnd(cell, width)).PadRight(width);
            });

            output.Add(new Components.ColoredLine(string.Join(" | ", formattedRow)));
        }

        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(bottomBar, ConsoleColor.Gray));
        output.Add(new Components.ColoredLine(statsBar, ConsoleColor.Gray));

        return output;
    }

    public AuthPromptScreen AuthPromptRender(int consoleWidth)
    {
        var lines = new List<Components.ColoredLine>();
        string title = "🟦 Welcome to Flexlib";
        string subtitle = "Please identify yourself to continue";

        // Layout sizes
        int innerWidth = Math.Min(consoleWidth - 8, 60);
        int paddingX = (consoleWidth - innerWidth) / 2;
        string padX = new(' ', paddingX);
        string horizontal = new('─', innerWidth - 2);

        // Content lines (excluding vertical centering)
        var boxLines = new List<Components.ColoredLine>();

        boxLines.Add(new Components.ColoredLine($"{padX}┌{horizontal}┐", ConsoleColor.DarkGray));
        boxLines.Add(new Components.ColoredLine($"{padX}│ {title.PadRight(innerWidth - 3)}│", ConsoleColor.Cyan));
        boxLines.Add(new Components.ColoredLine($"{padX}│ {subtitle.PadRight(innerWidth - 3)}│", ConsoleColor.Gray));
        boxLines.Add(new Components.ColoredLine($"{padX}│{"".PadRight(innerWidth - 2)}│", ConsoleColor.Gray));

        string idLabel = "🪪 ID:";
        string passLabel = "🔒 Password:";
        int labelPad = 4;
        string idLine = $"{new string(' ', labelPad)}{idLabel}";
        string passLine = $"{new string(' ', labelPad)}{passLabel}";

        int idX = paddingX + 1 + labelPad + idLabel.Length + 2; // +1 because of box border +2 to get distance from prompt text
        int idY = boxLines.Count;
        
        int passX = paddingX + 1 + labelPad + passLabel.Length + 2; // +1 because of box border +2 to get distance from prompt text
        boxLines.Add(new Components.ColoredLine($"{padX}│ {idLine.PadRight(innerWidth - 3)}│", ConsoleColor.White));
        int passY = boxLines.Count;
        boxLines.Add(new Components.ColoredLine($"{padX}│ {passLine.PadRight(innerWidth - 3)}│", ConsoleColor.White));

        boxLines.Add(new Components.ColoredLine($"{padX}│{"".PadRight(innerWidth - 2)}│", ConsoleColor.Gray));
        boxLines.Add(new Components.ColoredLine($"{padX}└{horizontal}┘", ConsoleColor.DarkGray));

        // Center vertically
        int consoleHeight = Console.WindowHeight;
        int verticalPad = Math.Max(0, (consoleHeight - boxLines.Count) / 2);

        for (int i = 0; i < verticalPad; i++)
            lines.Add(new Components.ColoredLine("", ConsoleColor.Gray)); // empty line for spacing

        lines.AddRange(boxLines);

        // Adjust Y positions after vertical padding
        return new AuthPromptScreen
        {
            Lines = lines,
            IDPosition = (idX, verticalPad + idY),
            PasswordPosition = (passX, verticalPad + passY)
        };
    }

    public RegistrationPromptScreen RegistrationPromptRender(int consoleWidth)
    {
        var lines = new List<Components.ColoredLine>();
        string title = "🟦 Welcome to Flexlib";
        string subtitle = "Please create your account";

        // Layout sizes
        int innerWidth = Math.Min(consoleWidth - 8, 60);
        int paddingX = (consoleWidth - innerWidth) / 2;
        string padX = new(' ', paddingX);
        string horizontal = new('─', innerWidth - 2);

        // Box content lines
        var boxLines = new List<Components.ColoredLine>();

        boxLines.Add(new($"{padX}┌{horizontal}┐", ConsoleColor.DarkGray));
        boxLines.Add(new($"{padX}│ {title.PadRight(innerWidth - 3)}│", ConsoleColor.Cyan));
        boxLines.Add(new($"{padX}│ {subtitle.PadRight(innerWidth - 3)}│", ConsoleColor.Gray));
        boxLines.Add(new($"{padX}│{"".PadRight(innerWidth - 2)}│", ConsoleColor.Gray));

        // Field labels
        int labelPad = 4;
        string nameLabel = "👤 Name:";
        string idLabel   = "🪪 ID:";
        string passLabel = "🔒 Password:";

        string nameLine = $"{new string(' ', labelPad)}{nameLabel}";
        string idLine   = $"{new string(' ', labelPad)}{idLabel}";
        string passLine = $"{new string(' ', labelPad)}{passLabel}";

        int nameX = paddingX + 1 + labelPad + nameLabel.Length + 2;
        int idX   = paddingX + 1 + labelPad + idLabel.Length + 2;
        int passX = paddingX + 1 + labelPad + passLabel.Length + 2;

        int nameY = boxLines.Count;
        boxLines.Add(new($"{padX}│ {nameLine.PadRight(innerWidth - 3)}│", ConsoleColor.White));

        int idY = boxLines.Count;
        boxLines.Add(new($"{padX}│ {idLine.PadRight(innerWidth - 3)}│", ConsoleColor.White));

        int passY = boxLines.Count;
        boxLines.Add(new($"{padX}│ {passLine.PadRight(innerWidth - 3)}│", ConsoleColor.White));

        boxLines.Add(new($"{padX}│{"".PadRight(innerWidth - 2)}│", ConsoleColor.Gray));
        boxLines.Add(new($"{padX}└{horizontal}┘", ConsoleColor.DarkGray));

        // Center vertically
        int consoleHeight = Console.WindowHeight;
        int verticalPad = Math.Max(0, (consoleHeight - boxLines.Count) / 2);

        for (int i = 0; i < verticalPad; i++)
            lines.Add(new("", ConsoleColor.Gray)); // blank padding

        lines.AddRange(boxLines);

        return new RegistrationPromptScreen
        {
            Lines = lines,
            NamePosition     = (nameX, verticalPad + nameY),
            IDPosition       = (idX, verticalPad + idY),
            PasswordPosition = (passX, verticalPad + passY)
        };
    }

}


