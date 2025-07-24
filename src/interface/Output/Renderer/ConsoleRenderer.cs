using Flexlib.Domain;
using System.Text.Json;

namespace Flexlib.Interface.Output;

public class Renderer
{
    public string Message(string? message)
    {
        return $"\n{message}\n";
    }
    
    public string ExplainUsage(string? usageInstructions)
    {
        return usageInstructions != null
            ? $"\n{usageInstructions}\n"
            : "\n Usage: flexlib {command} [options] \n";
    }

    public string Success(string message) => $"\n✔  {message}\n";

    public string Failure(string message) => $"\n✖  {message}\n";

    public string Error(string message) => $"\nError: {message}\n";

    public List<string> FormatCommentTable(List<Comment> comments, int consoleWidth)
    {
        string titleBar = "░░░░ COMMENTS " + new string('░', Math.Max(0, consoleWidth - 14));
        string bottomBar = new string('░', consoleWidth);

        const int padding = 3;
        const string ellipsis = "…";

        string Truncate(string text, int max)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= max ? text : text.Substring(0, Math.Max(0, max - 1)) + ellipsis;
        }

        var output = new List<string>();

        if (comments == null || comments.Count == 0)
        {
            output.Add("\nNo comments found.");
            return output;
        }

        var headers = new[] { "ID", "Text" };

        var rows = comments
            .Where(c => c != null)
            .Select(c => new[] { c.Id ?? "", c.Text.TrimEnd('\r', '\n') ?? "" })
            .ToList();

        // Step 1: Get minimum required width for ID
        int idWidth = rows.Select(r => r[0].Length).Append(headers[0].Length).Max();

        // Step 2: Deduct padding and ID width from total available space
        int availableTextWidth = consoleWidth - idWidth - padding;
        int maxTextContentWidth = rows.Select(r => r[1].Length).Append(headers[1].Length).Max();
        int textWidth = Math.Min(availableTextWidth, maxTextContentWidth);
        textWidth = Math.Max(headers[1].Length, textWidth);

        // Adjust ID again if total is still too big
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

        // Output header
        output.Add("");
        output.Add(titleBar);
        output.Add("");

        var headerLine = string.Join(" | ",
            headers.Select((h, i) => Truncate(h, colWidths[i]).PadRight(colWidths[i])));
        output.Add(headerLine);

        output.Add(new string('-', consoleWidth));

        // Output rows with custom dividers
        string dividerRow = new string('-', consoleWidth);
        foreach (var row in rows)
        {
            var formattedRow = string.Join(" | ",
                row.Select((cell, i) => Truncate(cell, colWidths[i]).PadRight(colWidths[i])));
            output.Add(formattedRow);

            output.Add(dividerRow);
        }

        output.Add("");
        output.Add(bottomBar);
        output.Add("");

        return output;
    }

    public List<string> FormatItemTable(List<LibraryItem> items, int consoleWidth)
    {
        string titleBar = "░░░░ LIBRARY ITEMS " + new string('░', Math.Max(0, consoleWidth - 20));
        string bottomBar = new string('░', consoleWidth);

        const int padding = 3;
        const string ellipsis = "…";

        // Helper to truncate
        string Truncate(string text, int max)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= max ? text : text.Substring(0, Math.Max(0, max - 1)) + ellipsis;
        }

        // Determine all unique property keys
        var allKeys = items
            .Where(i => i.PropertyValues != null)
            .SelectMany(i => i.PropertyValues.Keys)
            .Distinct()
            .OrderBy(k => k)
            .ToList();

        var headers = new[] { "Id", "Name" }.Concat(allKeys).ToList();
        int columnCount = headers.Count;

        // Step 1: Build all rows
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

        // Step 2: Ideal column widths
        int[] idealColWidths = new int[columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            int maxDataWidth = rows.Max(r => r[i]?.Length ?? 0);
            idealColWidths[i] = Math.Max(headers[i].Length, maxDataWidth);
        }

        int totalPadding = (columnCount - 1) * padding;
        int idealTotalWidth = idealColWidths.Sum() + totalPadding;

        // Step 3: Scale to fit
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

        // Step 4: Output formatting
        var formattedLines = new List<string>();

        formattedLines.Add("");
        formattedLines.Add(titleBar);
        formattedLines.Add("");

        // Header
        formattedLines.Add(string.Join(" | ",
            headers.Select((h, i) => Truncate(h, colWidths[i]).PadRight(colWidths[i]))));

        // Divider
        formattedLines.Add(string.Join("-|-", colWidths.Select(w => new string('-', w))));

        // Rows
        foreach (var row in rows)
        {
            formattedLines.Add(string.Join(" | ",
                row.Select((cell, i) => Truncate(cell, colWidths[i]).PadRight(colWidths[i]))));
        }

        formattedLines.Add("");
        formattedLines.Add(bottomBar);
        formattedLines.Add("");

        return formattedLines;
    }

    public List<string> FormatLibraryTable(List<Library> items, int consoleWidth)
    {
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

        // Step 1: Build rows
        var rows = items.Select(lib => new[]
        {
            lib.Name ?? "",
            lib.PropertyDefinitions != null ? string.Join(", ", lib.PropertyDefinitions.Select(p => p.Name)) : "",
            lib.LayoutSequence != null ? string.Join("/", lib.LayoutSequence.Select(l => l.Name)) : "",
            lib.Path ?? ""
        }).ToList();

        // Step 2: Determine ideal widths
        int[] idealWidths = headers.Select((h, i) =>
            Math.Max(h.Length, rows.Max(r => r[i]?.Length ?? 0))).ToArray();

        int totalPadding = (columnCount - 1) * padding;
        int totalIdeal = idealWidths.Sum() + totalPadding;

        int[] colWidths = new int[columnCount];
        Array.Copy(idealWidths, colWidths, columnCount);

        // Step 3: Shrink widths if necessary
        if (totalIdeal > consoleWidth)
        {
            int available = consoleWidth - totalPadding;
            int fixedCols = idealWidths[0] + idealWidths[1] + idealWidths[2];

            if (fixedCols + 10 > available)
            {
                // Shrink all columns proportionally
                double scale = (double)available / idealWidths.Sum();
                for (int i = 0; i < columnCount; i++)
                    colWidths[i] = Math.Max(6, (int)Math.Floor(idealWidths[i] * scale));

                // Adjust rounding
                int adjust = available - colWidths.Sum();
                for (int i = 0; adjust != 0 && i < columnCount; i++)
                {
                    colWidths[i] += Math.Sign(adjust);
                    adjust -= Math.Sign(adjust);
                }
            }
            else
            {
                // Fix first 3, shrink only Path
                colWidths[0] = idealWidths[0];
                colWidths[1] = idealWidths[1];
                colWidths[2] = idealWidths[2];
                colWidths[3] = available - (colWidths[0] + colWidths[1] + colWidths[2]);
            }
        }

        // Step 4: Compose output
        var output = new List<string>();
        output.Add("");
        output.Add(titleBar);
        output.Add("");

        output.Add(string.Join(" | ", headers.Select((h, i) => TruncateEnd(h, colWidths[i]).PadRight(colWidths[i]))));
        output.Add(string.Join("-|-", colWidths.Select(w => new string('-', w))));

        foreach (var row in rows)
        {
            var formattedRow = row.Select((cell, i) =>
            {
                var width = colWidths[i];
                return (i == 3 ? TruncateStart(cell, width) : TruncateEnd(cell, width)).PadRight(width);
            });

            output.Add(string.Join(" | ", formattedRow));
        }

        output.Add("");
        output.Add(bottomBar);
        output.Add("");

        return output;
    }

}


