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
            output.Add("No comments found.");
            return output;
        }

        var headers = new[] { "ID", "Text" };
        var rows = comments
            .Where(c => c != null)
            .Select(c => new[] { c.Id ?? "", c.Text ?? "" })
            .ToList();

        int idWidth = Math.Max(headers[0].Length, rows.Max(r => r[0].Length));
        int maxAvailableTextWidth = consoleWidth - idWidth - padding;
        int textWidth = Math.Min(maxAvailableTextWidth, rows.Max(r => r[1].Length));
        textWidth = Math.Max(headers[1].Length, textWidth);

        // Adjust ID width to match title
        idWidth = consoleWidth - textWidth - padding;

        var colWidths = new[] { idWidth, textWidth };

        output.Add("");
        output.Add(titleBar);
        output.Add("");

        // Header
        var headerLine = string.Join(" | ",
            headers.Select((h, i) => Truncate(h, colWidths[i]).PadRight(colWidths[i])));
        output.Add(headerLine);

        // Divider
        var dividerLine = string.Join("-|-", colWidths.Select(w => new string('-', w)));
        output.Add(dividerLine);

        // Rows with per-comment divider
        foreach (var row in rows)
        {
            var formattedLine = string.Join(" | ",
                row.Select((cell, i) => Truncate(cell, colWidths[i]).PadRight(colWidths[i])));
            output.Add(formattedLine);

            // Custom divider proportional to text column
            char dash = '-';
            int dashWidth = consoleWidth;
            output.Add(new string(dash, dashWidth));
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
        string titleBar = "░░░░ LIBRARIES " + new string('░', consoleWidth - 15);
        string bottomBar = new string('░', consoleWidth);

        var headers = new[] { "Name", "Properties", "Layout", "Path" };
        int columnCount = headers.Length;
        const int padding = 3; // " | " between columns
        const string ellipsis = "…";

        // Helper to safely truncate
        string Truncate(string text, int max)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= max ? text : text.Substring(0, Math.Max(0, max - 1)) + ellipsis;
        }

        // Step 1: Materialize all rows
        var rows = new List<string[]>();

        foreach (var lib in items)
        {
            if (lib == null) continue;

            var name = lib.Name ?? "";
            var properties = lib.PropertyDefinitions != null
                ? string.Join(", ", lib.PropertyDefinitions.Select(p => p.Name))
                : "";

            var layout = lib.LayoutSequence != null
                ? string.Join("/", lib.LayoutSequence.Select(l => l.Name))
                : "";

            var path = lib.Path ?? "";

            rows.Add(new[] { name, properties, layout, path });
        }

        // Step 2: Compute ideal column widths
        int[] idealColWidths = new int[columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            int maxDataWidth = rows.Max(r => r[i]?.Length ?? 0);
            idealColWidths[i] = Math.Max(headers[i].Length, maxDataWidth);
        }

        int totalPadding = (columnCount - 1) * padding;
        int idealTotalWidth = idealColWidths.Sum() + totalPadding;

        // Step 3: Fit to console width if needed
        int[] colWidths = new int[columnCount];
        if (idealTotalWidth <= consoleWidth)
        {
            // Use ideal widths
            Array.Copy(idealColWidths, colWidths, columnCount);
        }
        else
        {
            // Shrink proportionally
            int availableWidth = consoleWidth - totalPadding;

            // Start with ideal → scaled down
            double scale = (double)availableWidth / idealColWidths.Sum();

            for (int i = 0; i < columnCount; i++)
            {
                colWidths[i] = Math.Max(6, (int)Math.Floor(idealColWidths[i] * scale));
            }

            // Adjust any rounding differences
            int adjustedWidth = colWidths.Sum();
            int diff = availableWidth - adjustedWidth;

            for (int i = 0; diff != 0 && i < columnCount; i++)
            {
                int adjust = diff > 0 ? 1 : -1;
                colWidths[i] += adjust;
                diff -= adjust;
            }
        }

        // Step 4: Format title, header and rows
        var formattedLines = new List<string>();
        
        formattedLines.Add("");
        formattedLines.Add(titleBar);
        formattedLines.Add("");

        // Header
        var headerLine = string.Join(" | ",
            headers.Select((h, i) => Truncate(h, colWidths[i]).PadRight(colWidths[i])));
        formattedLines.Add(headerLine);

        // Divider
        var dividerLine = string.Join("-|-", colWidths.Select(w => new string('-', w)));
        formattedLines.Add(dividerLine);

        // Rows
        foreach (var row in rows)
        {
            var line = string.Join(" | ",
                row.Select((cell, i) => Truncate(cell, colWidths[i]).PadRight(colWidths[i])));
            formattedLines.Add(line);
        }
        
        formattedLines.Add("");
        formattedLines.Add(bottomBar);
        formattedLines.Add("");

        return formattedLines;
    }


}


