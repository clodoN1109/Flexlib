using Flexlib.Domain;
using System.Text.Json;
using System.Drawing;
using Flexlib.Application.Ports;
using Flexlib.Interface.CLI;
using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Processing;
using Flexlib.Interface.Output;
using Flexlib.Interface.Input;
using System.Linq;

namespace Flexlib.Interface.Output;


public partial class ConsoleRenderer
{

    public List<Components.ColoredLine> FormatLoanHistoryTable( LoanHistory history, LibraryItem item, string libName, int consoleWidth )
    {
        var output = new List<Components.ColoredLine>();

        string logoBar   = Components.LogoLine(consoleWidth);
        string titleBar  = "░░░░ LOAN HISTORY " + new string('░', Math.Max(0, consoleWidth - 20));
        string header    = Components.LineFilled(consoleWidth, "left", ' ', $"{libName}/{item.Name ?? $"#{item.Id}"}");
        string statsBar  = Components.LineFilled(consoleWidth, "right", ' ', $"{history.Entries.Count} entr{(history.Entries.Count == 1 ? "y" : "ies")}");
        string bottomBar = new string('░', consoleWidth);

        var tableHeaders = new[] { "BORROWED AT", "RETURNED AT", "BORROWER" };
        const int padding = 3;
        const string ellipsis = "…";

        var rows = history.Entries
            .Select(entry => new[]
            {
                entry.BorrowedAt.ToString("yyyy-MM-dd"),
                entry.WasReturned && entry.ReturnedAt.HasValue
                    ? entry.ReturnedAt.Value.ToString("yyyy-MM-dd")
                    : "—",
                entry.UserId ?? "—",
            })
            .ToList();

        string Truncate(string text, int max) =>
            string.IsNullOrEmpty(text) ? "" : text.Length <= max ? text : text[..Math.Max(0, max - 1)] + ellipsis;

        int[] colWidths = new int[tableHeaders.Length];
        for (int i = 0; i < tableHeaders.Length; i++)
        {
            int maxRowLength = rows.Any()
                ? rows.Max(r => r[i].Split('\n').Max(line => line.Length))
                : 0;

            colWidths[i] = Math.Max(tableHeaders[i].Length, maxRowLength);
        }

        int totalPadding = (tableHeaders.Length - 1) * padding;
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

        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(logoBar));
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(titleBar, ConsoleColor.Gray));
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(header));
        output.Add(new Components.ColoredLine(""));

        var headerLine = string.Join(" | ",
            tableHeaders.Select((h, i) => Truncate(h, colWidths[i]).PadRight(colWidths[i])));
        output.Add(new Components.ColoredLine(headerLine, ConsoleColor.DarkGray));
        output.Add(new Components.ColoredLine(new string('-', consoleWidth), ConsoleColor.DarkGray));

        foreach (var row in rows)
        {
            var splitCells = row.Select((cell, i) =>
                cell.Split('\n')
                    .Select(line => Truncate(line, colWidths[i]).PadRight(colWidths[i]))
                    .ToList()
            ).ToList();

            int maxLines = splitCells.Max(lines => lines.Count);

            for (int i = 0; i < splitCells.Count; i++)
            {
                while (splitCells[i].Count < maxLines)
                    splitCells[i].Add(new string(' ', colWidths[i]));
            }

            for (int line = 0; line < maxLines; line++)
            {
                var formattedLine = string.Join(" | ",
                    splitCells.Select(col => col[line]));
                output.Add(new Components.ColoredLine(formattedLine));
            }

            output.Add(new Components.ColoredLine(new string('-', consoleWidth), ConsoleColor.DarkGray));
        }

        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(bottomBar, ConsoleColor.Gray));
        output.Add(new Components.ColoredLine(statsBar));

        return output;
    }


    public List<Components.ColoredLine> FormatNoteTable(List<Note> notes, string itemName, string libName, int consoleWidth)
    {
        var output = new List<Components.ColoredLine>();

        string logoBar   = Components.LogoLine(consoleWidth);
        string titleBar  = "░░░░ NOTES " + new string('░', Math.Max(0, consoleWidth - 14));
        string header    = Components.LineFilled(consoleWidth, "left", ' ', $"{libName}/{itemName}");
        string statsBar  = Components.LineFilled(consoleWidth, "right", ' ', $"{notes.Count} notes");
        string bottomBar = new string('░', consoleWidth);

        var tableHeaders = new[] { "ID", "AUTHOR", "TEXT", "CREATED AT", "EDITED AT" };
        const int padding = 3;
        const string ellipsis = "…";

        // Build table rows
        var rows = notes
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
        int[] colWidths = new int[tableHeaders.Length];
        for (int i = 0; i < tableHeaders.Length; i++)
        {
            int maxRowLength = rows.Any()
                ? rows.Max(r => r[i].Split('\n').Max(line => line.Length))
                : 0;

            colWidths[i] = Math.Max(tableHeaders[i].Length, maxRowLength);
        }

        // Adjust widths to fit console
        int totalPadding = (tableHeaders.Length - 1) * padding;
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
            tableHeaders.Select((h, i) => Truncate(h, colWidths[i]).PadRight(colWidths[i])));
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
        string header = Components.LineFilled(
            consoleWidth,
            "left",
            ' ',
            $"{lib.Name}/{filterSequence}/{string.Join('|', itemNameFilter.Where(n => n != "*").Select(n => n.IsCompound() ? $"'{n}'" : n))}",
            $"{sortSequence}"
        );

        string stats = $"{items.Count} items" + " " + $"{localSizeInBytes:N2} bytes";
        string footer = Components.LineSpacedBetween(consoleWidth, layoutSequence, stats);
        
        string bottomBar = new string('░', consoleWidth);

        const int padding = 3;
        const string ellipsis = "…";

        string Truncate(string text, int max) =>
            string.IsNullOrEmpty(text) ? "" : text.Length <= max ? text : text[..Math.Max(0, max - 1)] + ellipsis;

        var allKeys = lib
            .PropertyDefinitions
            .Select(d => d.Name)
            .OrderBy(k => k)
            .ToList();

        var tableHeaders = new[] { "ID", "NAME" }.Concat(allKeys).ToList();
        int columnCount = tableHeaders.Count;

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
                idealColWidths[i] = Math.Max(tableHeaders[i].Length, maxDataWidth);
            }
        }
        else {
            for (int i = 0; i < columnCount; i++)
            {
                idealColWidths[i] = tableHeaders[i].Length;

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
            tableHeaders.Select((h, i) => Truncate(h, colWidths[i]).PadRight(colWidths[i]))), ConsoleColor.DarkGray));

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
            ("NAME", lib => lib.Name ?? "", false),
            ("ITEMS", lib => lib.Items?.Count.ToString() ?? "0", false),
            ("PROPERTIES", lib => lib.PropertyDefinitions != null ? string.Join(", ", lib.PropertyDefinitions.Select(p => p.Name)) : "", false),
            ("LAYOUT", lib => lib.LayoutSequence != null ? string.Join("/", lib.LayoutSequence.Select(l => l.Name)) : "", true),
            ("LOCATION", lib => lib.Path ?? "", false)
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


    public List<Components.ColoredLine> FormatItemPropertiesTable(LibraryItem item, Library lib, int consoleWidth)
    {
        var output = new List<Components.ColoredLine>();

        string logoBar = Components.LogoLine(consoleWidth);
        string titleBar = $"░░░░ ITEM PROPERTIES {new string('░', Math.Max(0, consoleWidth - 21))}";
        string header = Components.LineFilled(consoleWidth,
                "left", ' ', 
                $"{lib.Name!}/{ ( item.Name!.IsCompound() ? $"\'{item.Name}\'" : item.Name! ) }" );

        string statsBar  = Components.LineSpacedBetween(consoleWidth, $"Item ID: {item.Id}", $"{lib.PropertyDefinitions.Count} properties");
        string bottomBar = new string('░', consoleWidth);

        const int padding = 4;
        const string ellipsis = "…";

        string Truncate(string text, int max) =>
            string.IsNullOrEmpty(text) ? "" : text.Length <= max ? text : text[..Math.Max(0, max - 1)] + ellipsis;

        var properties = lib.PropertyDefinitions
            .Select(d => d.Name)
            .OrderBy(name => name)
            .ToList();

        var propertyValues = item.GetPropertyValuesAsListOfStrings();

        var rows = properties
            .Select(name => new[] {
                name,
                propertyValues.TryGetValue(name, out var values) && values != null
                    ? string.Join(", ", values)
                    : ""
            })
            .ToList();

        int[] idealColWidths = new int[2];
        if (rows.Count > 0) {
            idealColWidths[0] = Math.Max("PROPERTY".Length, rows.Max(r => r[0]?.Length ?? 0));
            idealColWidths[1] = Math.Max("VALUE".Length, rows.Max(r => r[1]?.Length ?? 0));
        }
        else {
            idealColWidths[0] = "PROPERTY".Length;
            idealColWidths[1] = "VALUE".Length;
        }

        int totalPadding = padding;
        int idealTotalWidth = idealColWidths.Sum() + totalPadding;

        int[] colWidths = new int[2];
        if (idealTotalWidth <= consoleWidth)
        {
            Array.Copy(idealColWidths, colWidths, 2);
        }
        else
        {
            int availableWidth = consoleWidth - totalPadding;
            double scale = (double)availableWidth / idealColWidths.Sum();

            for (int i = 0; i < 2; i++)
                colWidths[i] = Math.Max(6, (int)Math.Floor(idealColWidths[i] * scale));

            int diff = availableWidth - colWidths.Sum();
            for (int i = 0; diff != 0 && i < 2; i++)
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

        output.Add(new Components.ColoredLine(
            Truncate("PROPERTY", colWidths[0]).PadRight(colWidths[0]) + " │ " +
            Truncate("VALUE", colWidths[1]).PadRight(colWidths[1]),
            ConsoleColor.DarkGray));

        output.Add(new Components.ColoredLine(
            new string('-', colWidths[0]) + "-┼-" + new string('-', colWidths[1]),
            ConsoleColor.DarkGray));

        foreach (var row in rows)
        {
            output.Add(new Components.ColoredLine(
                Truncate(row[0], colWidths[0]).PadRight(colWidths[0]) + " │ " +
                Truncate(row[1], colWidths[1]).PadRight(colWidths[1])));
        }

        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(bottomBar, ConsoleColor.Gray));
        output.Add(new Components.ColoredLine(statsBar, ConsoleColor.Gray));

        return output;
    }

    public List<Components.ColoredLine> FormatPropertyDefinitionsTable(Library lib, int consoleWidth)
    {
        var output = new List<Components.ColoredLine>();

        string logoBar = Components.LogoLine(consoleWidth);
        string titleBar = $"░░░░ PROPERTY DEFINITIONS {new string('░', Math.Max(0, consoleWidth - 26))}";
        string header = Components.LineFilled(consoleWidth, "left", ' ', lib.Name!);
        string statsBar  = Components.LineFilled(consoleWidth, "right", ' ', $"{lib.PropertyDefinitions.Count} properties");
        string bottomBar = new string('░', consoleWidth);

        const int padding = 4;
        const string ellipsis = "…";

        string Truncate(string text, int max) =>
            string.IsNullOrEmpty(text) ? "" : text.Length <= max ? text : text[..Math.Max(0, max - 1)] + ellipsis;

        var rows = lib.PropertyDefinitions
            .OrderBy(d => d.Name)
            .Select(d => new[] {
                d.Name ?? "",
                d.TypeName ?? "",
                ""
            })
            .ToList();

        var tableHeaders = new[] { "NAME", "TYPE", "DESCRIPTION" };
        int columnCount = tableHeaders.Length;

        int[] idealColWidths = new int[columnCount];
        if (rows.Count > 0) {
            for (int i = 0; i < columnCount; i++)
                idealColWidths[i] = Math.Max(tableHeaders[i].Length, rows.Max(r => r[i]?.Length ?? 0));
        }
        else {
            for (int i = 0; i < columnCount; i++)
                idealColWidths[i] = tableHeaders[i].Length;
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

        output.Add(new Components.ColoredLine(string.Join(" │ ",
            tableHeaders.Select((h, i) => Truncate(h, colWidths[i]).PadRight(colWidths[i]))), ConsoleColor.DarkGray));

        output.Add(new Components.ColoredLine(string.Join("-┼-", colWidths.Select(w => new string('-', w))), ConsoleColor.DarkGray));

        foreach (var row in rows)
        {
            output.Add(new Components.ColoredLine(string.Join(" │ ",
                row.Select((cell, i) => Truncate(cell, colWidths[i]).PadRight(colWidths[i])))));
        }

        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(bottomBar, ConsoleColor.Gray));
        output.Add(new Components.ColoredLine(statsBar, ConsoleColor.Gray));

        return output;
    }

    public List<Components.ColoredLine> FormatDesksTable(List<Desk> desks, string libraryName, int consoleWidth)
    {
        var output = new List<Components.ColoredLine>();

        string logoBar   = Components.LogoLine(consoleWidth);
        string titleBar  = $"░░░░ DESKS " + new string('░', Math.Max(0, consoleWidth - 11));
        string header = Components.LineFilled(consoleWidth, "left", ' ', libraryName); 
        string statsBar  = Components.LineFilled(consoleWidth, "right", ' ', $"{desks.Count} desks");
        string bottomBar = new string('░', consoleWidth);

        const int padding = 3;
        const string ellipsis = "…";

        string TruncateEnd(string text, int max) =>
            string.IsNullOrEmpty(text) || text.Length <= max ? text : text[..Math.Max(0, max - 1)] + ellipsis;

        string TruncateStart(string text, int max) =>
            string.IsNullOrEmpty(text) || text.Length <= max ? text : ellipsis + text[^Math.Max(0, max - 1)..];

        var columns = new List<(string Header, Func<Desk, string> ValueSelector, bool TruncateStart)>
        {
            ("ID",              d => d.Id.ToString(), false),
            ("NAME",            d => d.Name ?? "",    false),
            ("BORROWED ITEMS",  d => d.BorrowedItems.Count.ToString(), false),
        };

        int columnCount = columns.Count;

        var rows = desks.Select(d => columns.Select(col => col.ValueSelector(d)).ToArray()).ToList();

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

        // Render
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(logoBar));
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(titleBar, ConsoleColor.Gray));
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(header, ConsoleColor.DarkGray));
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
    
    public List<Components.ColoredLine> FormatDeskTable(Desk desk, string libName, int consoleWidth)
    {
        var output = new List<Components.ColoredLine>();

        string logoBar    = Components.LogoLine(consoleWidth);
        string titleBar   = $"░░░░ DESK VIEW {new string('░', Math.Max(0, consoleWidth - 15))}";
        string headerLine = Components.LineFilled(consoleWidth, "left", ' ', 
                $"{libName}/{ ( desk.Name!.IsCompound() ? $"\'{desk.Name}\'" : desk.Name)}");

        string bottomBar  = new string('░', consoleWidth);

        const int padding = 3;
        const string ellipsis = "…";

        string Truncate(string text, int max) =>
            string.IsNullOrEmpty(text) ? "" : text.Length <= max ? text : text[..Math.Max(0, max - 1)] + ellipsis;

        var tableHeaders = new[] { "ID", "NAME", "BORROWED AT", "APPETITE", "PROGRESS", "PRIORITY" };
        int columnCount = tableHeaders.Length;

        var rows = new List<string[]>();
        foreach (var item in desk.BorrowedItems)
        {
            string progress = FormatProgress(item.Progress);
            rows.Add(new[]
            {
                item.Id ?? "",
                item.Name ?? "",
                item.BorrowedAt?.ToString("yyyy-MM-dd HH:mm") ?? "",
                item.Appetite?.ToString("yyyy-MM-dd HH:mm") ?? "",
                progress,
                item.Priority.ToString()
            });
        }

        // Calculate ideal column widths
        int[] idealColWidths = new int[columnCount];
        if (rows.Count > 0)
        {
            for (int i = 0; i < columnCount; i++)
            {
                int maxDataWidth = rows.Max(r => r[i]?.Length ?? 0);
                idealColWidths[i] = Math.Max(tableHeaders[i].Length, maxDataWidth);
            }
        }
        else
        {
            for (int i = 0; i < columnCount; i++)
            {
                idealColWidths[i] = tableHeaders[i].Length;
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

        // Render output
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(logoBar));
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(titleBar, ConsoleColor.Gray));
        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(headerLine, ConsoleColor.DarkGray));
        output.Add(new Components.ColoredLine(""));

        if (!rows.Any())
        {
            output.Add(new Components.ColoredLine(TextUtil.CenterText("No borrowed items.", consoleWidth)));
        }
        else
        {
            output.Add(new Components.ColoredLine(string.Join(" | ",
                tableHeaders.Select((h, i) => Truncate(h, colWidths[i]).PadRight(colWidths[i]))), ConsoleColor.DarkGray));

            output.Add(new Components.ColoredLine(string.Join("-|-",
                colWidths.Select(w => new string('-', w))), ConsoleColor.DarkGray));

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                bool isCompleted = desk.BorrowedItems[i].Progress.IsCompleted;

                var line = string.Join(" | ",
                    row.Select((cell, j) => Truncate(cell, colWidths[j]).PadRight(colWidths[j])));

                output.Add(new Components.ColoredLine(line, isCompleted ? ConsoleColor.Green : ConsoleColor.Gray));
            }

        }

        output.Add(new Components.ColoredLine(""));
        output.Add(new Components.ColoredLine(bottomBar, ConsoleColor.Gray));

        string footer = Components.LineSpacedBetween(consoleWidth,
            $"Desk ID: {desk.Id}",
            $"{desk.BorrowedItems.Count} items"
            );

        output.Add(new Components.ColoredLine(footer, ConsoleColor.DarkGray));

        return output;
    }

    private static string FormatProgress(BorrowedItem.ProgressVariable progress)
    {
        if (string.IsNullOrEmpty(progress.CurrentValue) && string.IsNullOrEmpty(progress.CompletionValue))
            return "";

        return $"{progress.CurrentValue}/{progress.CompletionValue} {progress.Unit}";
    }

}


