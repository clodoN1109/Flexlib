using Flexlib.Domain;

namespace Flexlib.Interface.Output;


public partial class ConsoleRenderer
{
    public static List<Components.ColoredRow> RenderTable(
        int consoleWidth,
        IList<string> tableHeaders,
        IList<string[]> rows,
        string ellipsis = "…",
        int maxCellWidth = 50,
        int padding = 3,
        bool fitToConsole = false
    )
    {
        var output = new List<Components.ColoredRow>();

        // --- helpers -------------------------------------------------------------
        string Truncate(string text, int max)
        {
            if (max <= 0) return ""; // safe guard
            if (string.IsNullOrEmpty(text) || text.Length <= max) return text ?? "";
            if (max <= ellipsis.Length) return ellipsis.Substring(0, max);
            return text.Substring(0, max - ellipsis.Length) + ellipsis;
        }

        static int EffectivePadding(int requested, int consoleWidth, int columnCount)
        {
            if (columnCount <= 1) return 0;
            if (consoleWidth <= 0) return 0;
            var maxPad = Math.Max(0, (consoleWidth - columnCount) / (columnCount - 1));
            return Math.Min(Math.Max(0, requested), maxPad);
        }

        // --- guards --------------------------------------------------------------
        int columnCount = tableHeaders?.Count ?? 0;
        if (columnCount <= 0 || consoleWidth <= 0)
            return output;

        // Make local mutable copies
        var headers = tableHeaders?.ToList() ?? new List<string>();
        var bodyRows = (rows ?? Array.Empty<string[]>()).Select(r => r ?? Array.Empty<string>()).ToList();

        // --- measure natural widths (max visible content per column) -------------
        int[] natural = new int[columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            int maxDataWidth = 0;
            if (bodyRows.Count > 0)
            {
                foreach (var r in bodyRows)
                {
                    string cell = i < r.Length ? r[i] ?? "" : "";
                    int longest = cell.Split('\n').Select(l => l.Length).DefaultIfEmpty(0).Max();
                    if (longest > maxDataWidth) maxDataWidth = longest;
                }
            }
            natural[i] = Math.Max(headers[i]?.Length ?? 0, maxDataWidth);
        }

        // desired = what the column could usefully take to show everything (bounded by maxCellWidth)
        int[] desired = natural
            .Select(n => Math.Max(1, Math.Min(n, Math.Max(1, maxCellWidth))))
            .ToArray();

        // --- layout math ---------------------------------------------------------
        int colPadding = EffectivePadding(padding, consoleWidth, columnCount);

        List<int> includedWidths;
        int includedCount;

        if (!fitToConsole)
        {
            // Ignore consoleWidth entirely, include all columns with their desired widths
            includedWidths = desired.ToList();
            includedCount = columnCount;
        }
        else
        {
            // Greedy include-left-to-right algorithm with consoleWidth limitation
            includedWidths = new List<int>();
            int usedWidth = 0;

            for (int i = 0; i < columnCount; i++)
            {
                int sepIfIncluded = Math.Max(0, includedWidths.Count) * colPadding;
                int availableIfIncluded = consoleWidth - sepIfIncluded;
                if (availableIfIncluded <= 0) break;

                int want = desired[i];

                if (usedWidth + want <= availableIfIncluded)
                {
                    includedWidths.Add(want);
                    usedWidth += want;
                    continue;
                }

                int remainingForThis = availableIfIncluded - usedWidth;
                if (remainingForThis >= 1)
                {
                    int give = Math.Min(remainingForThis, want);
                    includedWidths.Add(give);
                    usedWidth += give;
                }
                break;
            }

            includedCount = includedWidths.Count;

            // Drop rightmost columns if needed
            if (includedCount < columnCount)
            {
                headers = headers.Take(includedCount).ToList();
                bodyRows = bodyRows.Select(r => r.Take(includedCount).ToArray()).ToList();
                columnCount = includedCount;
                natural = natural.Take(includedCount).ToArray();
                desired = desired.Take(includedCount).ToArray();
            }
        }

        if (columnCount == 0)
            return output;

        // Compute separator widths and trailing fill
        int sepWidth = Math.Max(0, (columnCount - 1) * colPadding);
        int renderedBodyWidth = includedWidths.Sum();
        int renderedWidth = renderedBodyWidth + sepWidth;
        int trailingFill = Math.Max(0, consoleWidth - renderedWidth);

        string colSep = new string(' ', colPadding);
        string dashSep = new string('-', colPadding);

        // --- render --------------------------------------------------------------

        // Header row
        var headerLine = string.Join(colSep, headers.Select((h, i) => Truncate(h ?? "", includedWidths[i]).PadRight(includedWidths[i])))
                        + new string(' ', trailingFill);
        output.Add(new Components.ColoredRow(headerLine, ConsoleColor.DarkGray));

        // Separator row
        var separatorLine = string.Join(dashSep, includedWidths.Select(w => new string('-', w)))
                            + new string(' ', trailingFill);
        output.Add(new Components.ColoredRow(separatorLine, ConsoleColor.DarkGray));

        // Body rows
        foreach (var row in bodyRows)
        {
            var splitCells = Enumerable.Range(0, columnCount)
                .Select(i =>
                {
                    string cell = i < row.Length ? row[i] ?? "" : "";
                    return cell.Split('\n')
                            .Select(line => Truncate(line, includedWidths[i]).PadRight(includedWidths[i]))
                            .ToList();
                })
                .ToList();

            int maxLines = splitCells.Max(lines => lines.Count);

            for (int i = 0; i < splitCells.Count; i++)
            {
                while (splitCells[i].Count < maxLines)
                    splitCells[i].Add(new string(' ', includedWidths[i]));
            }

            for (int line = 0; line < maxLines; line++)
            {
                var formattedLine = string.Join(colSep, splitCells.Select(col => col[line]))
                                    + new string(' ', trailingFill);
                output.Add(new Components.ColoredRow(formattedLine));
            }

            output.Add(new Components.ColoredRow(separatorLine, ConsoleColor.DarkGray));
        }

        return output;
    }

    public List<Components.ColoredRow> RenderLibrariesTable(List<Library> libraries, int consoleWidth, bool fitToConsole = false)
    {
        var headers = new[] { "NAME", "ITEMS", "PROPERTIES", "LAYOUT", "LOCATION" }.ToList().TranslateToProfile();

        var rows = libraries.Select(lib => new[]
        {
                lib.Name ?? "",
                lib.Items?.Count.ToString() ?? "0",
                lib.PropertyDefinitions != null
                    ? string.Join(", ", lib.PropertyDefinitions.Select(p => p.Name))
                    : "",
                lib.LayoutSequence != null
                    ? string.Join("/", lib.LayoutSequence.Select(l => l.Name))
                    : "",
                lib.Path ?? ""
            }).ToList();

        var table = RenderTable(consoleWidth, headers, rows, "...", 50, 3, fitToConsole);

        return table;
    }

    public List<Components.ColoredRow> RenderDeskItemsTable(Desk desk, int consoleWidth, bool fitToConsole = false)
    {

        var tableHeaders = new[] { "ID", "NAME", "BORROWED AT", "APPETITE", "PROGRESS", "PRIORITY" }.ToList().TranslateToProfile();

        var rows = new List<string[]>();
        foreach (var item in desk.BorrowedItems)
        {
            string progress = RenderProgress(item.Progress);
            rows.Add(new[]
            {
                item.Id ?? "",
                item.Name ?? "",
                item.BorrowedAt?.ToLocalTime().ToString("MM-dd-yyyy HH:mm") ?? "",
                item.Appetite?.ToLocalTime().ToString("MM-dd-yyyy HH:mm") ?? "",
                progress,
                item.Priority.ToString()
            });

        }
        return RenderTable(consoleWidth, tableHeaders, rows, "...", 50, 3, fitToConsole);
    }
    string RenderProgress(BorrowedItem.ProgressVariable progress)
    {
        if (string.IsNullOrEmpty(progress.CurrentValue) && string.IsNullOrEmpty(progress.CompletionValue))
            return "";

        return $"{progress.CurrentValue}/{progress.CompletionValue} {progress.Unit}";
    }

    public List<Components.ColoredRow> RenderNoteTable(List<Note> notes, string itemName, int itemId, string libName, int consoleWidth, bool fitToConsole = false)
    {
        var tableHeaders = new[] { "ID", "AUTHOR", "TEXT", "CREATED AT", "EDITED AT" }.ToList().TranslateToProfile();

        // Keep \n for multiline cell rendering; strip only \r
        var rows = notes
            .Where(n => n != null)
            .Select(n => new[]
            {
            n.Id?.ToString() ?? "",
            n.Author?.Name ?? "",
            (n.Text ?? "").Replace("\r", ""),
            n.CreatedTime ?? "",
            n.EditedTime ?? ""
            })
            .ToList();

        return RenderTable(consoleWidth, tableHeaders, rows, "...", 50, 3, fitToConsole);
    }

    public List<Components.ColoredRow> RenderItemsTable(
    List<LibraryItem> items, Library lib, int consoleWidth, bool fitToConsole = false)
    {
        var allKeys = lib.PropertyDefinitions.Select(d => d.Name).OrderBy(k => k).ToList();
        var tableHeaders = new[] { "ID", "NAME" }.ToList().TranslateToProfile().Concat(allKeys).ToList();

        var rows = new List<string[]>();
        foreach (var item in items)
        {
            var row = new string[tableHeaders.Count];
            row[0] = item.Id.ToString() ?? "";
            row[1] = item.Name ?? "";

            var propertyDict = item.GetPropertyValuesAsListOfStrings();
            for (int i = 0; i < allKeys.Count; i++)
            {
                var key = allKeys[i];
                row[i + 2] = propertyDict.TryGetValue(key, out var values) && values != null
                    ? string.Join(", ", values)
                    : "";
            }

            rows.Add(row);
        }

        return RenderTable(consoleWidth, tableHeaders, rows, "...", 50, 3, fitToConsole);
    }

    public List<Components.ColoredRow> RenderDesksTable(List<Desk> desks, int consoleWidth, bool fitToConsole = false)
    {
        var tableHeaders = new[] { "ID", "NAME", "BORROWED ITEMS" }.ToList().TranslateToProfile();

        var rows = desks.Select(d => new[]
                                {
                                d.Id.ToString(),
                                d.Name ?? "",
                                d.BorrowedItems.Count.ToString()
                                }).ToList();

        return RenderTable(consoleWidth, tableHeaders, rows, "...", 50, 3, fitToConsole);
    }

    public List<Components.ColoredRow> RenderPropertyDefinitionsTable(Library lib, int consoleWidth, bool fitToConsole = false)
    {
        var tableHeaders = new[] { "NAME", "TYPE", "DESCRIPTION" }.ToList().TranslateToProfile();

        var rows = lib.PropertyDefinitions
            .OrderBy(d => d.Name)
            .Select(d => new[]
            {
                d.Name ?? "",
                d.TypeName ?? "",
                "" // placeholder for description
            })
            .ToList();
        return RenderTable(consoleWidth, tableHeaders, rows, "...", 50, 3, fitToConsole);
    }

    public List<Components.ColoredRow> RenderItemPropertiesTable(LibraryItem item, Library lib, int consoleWidth, bool fitToConsole = false)
    {
        var tableHeaders = new[] { "PROPERTY", "VALUE" }.ToList().TranslateToProfile();

        var propertyValues = item.GetPropertyValuesAsListOfStrings();
        var rows = lib.PropertyDefinitions
            .OrderBy(d => d.Name)
            .Select(d => new[]
            {
                d.Name ?? "",
                propertyValues.TryGetValue(d.Name!, out var values) && values != null
                    ? string.Join(", ", values)
                    : ""
            })
            .ToList();
        return RenderTable(consoleWidth, tableHeaders, rows, "...", 50, 3, fitToConsole);
    }
    public List<Components.ColoredRow> RenderLoanHistoryTable(LoanHistory history, LibraryItem item, string libName, int consoleWidth, bool fitToConsole = false)
    {
        var tableHeaders = new[] { "BORROWED AT", "RETURNED AT", "BORROWER" }.ToList().TranslateToProfile();

        var rows = history.Entries
            .Select(entry => new[]
            {
                entry.BorrowedAt.ToLocalTime().ToString("MM-dd-yyyy"),
                entry.WasReturned && entry.ReturnedAt.HasValue
                    ? entry.ReturnedAt.Value.ToLocalTime().ToString("MM-dd-yyyy")
                    : "—",
                entry.UserId ?? "—",
            })
            .ToList();
        return RenderTable(consoleWidth, tableHeaders, rows, "...", 50, 3, fitToConsole);
    }

}