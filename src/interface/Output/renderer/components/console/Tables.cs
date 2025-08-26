using Flexlib.Domain;

namespace Flexlib.Interface.Output;


public partial class ConsoleRenderer
{
    public static List<Components.ColoredRow> RenderTable(
        int consoleWidth,
        IList<string> tableHeaders,
        IList<string[]> rows,
        string ellipsis = "…",
        int maxCellWidth = 600,
        int padding = 3
    )

    {
        var output = new List<Components.ColoredRow>();

        // --- helpers -------------------------------------------------------------
        string Truncate(string text, int max)
        {
            if (max <= 0) return ""; // never happens in normal flow, but safe
            if (string.IsNullOrEmpty(text) || text.Length <= max) return text ?? "";
            if (max == 1) return ellipsis; // single cell char -> show ellipsis directly
            return text[..Math.Max(0, max - 1)] + ellipsis;
        }

        static int EffectivePadding(int requested, int consoleWidth, int columnCount)
        {
            if (columnCount <= 1) return 0;
            if (consoleWidth <= 0) return 0;
            // ensure there's room for at least 1 char per column
            var maxPad = Math.Max(0, (consoleWidth - columnCount) / (columnCount - 1));
            return Math.Min(Math.Max(0, requested), maxPad);
        }

        // --- guards --------------------------------------------------------------
        int columnCount = tableHeaders?.Count ?? 0;
        if (columnCount <= 0 || consoleWidth <= 0)
            return output;

        // We’ll possibly replace these if the console is too narrow.
        var headers = tableHeaders?.ToList() ?? new List<string>();
        var bodyRows = (rows ?? Array.Empty<string[]>()).Select(r => r ?? Array.Empty<string>()).ToList();

        // --- measure natural widths (max visible content per column) -------------
        int[] natural = new int[columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            int maxDataWidth = 0;
            if (bodyRows.Count > 0)
            {
                // handle multi-line cells; take the longest line
                foreach (var r in bodyRows)
                {
                    string cell = i < r.Length ? r[i] ?? "" : "";
                    int longest = cell.Split('\n').Select(l => l.Length).DefaultIfEmpty(0).Max();
                    if (longest > maxDataWidth) maxDataWidth = longest;
                }
            }
            natural[i] = Math.Max(headers[i]?.Length ?? 0, maxDataWidth);
        }

        // desired = what the column could usefully take to show everything
        int[] desired = natural.Select(n => Math.Max(1, Math.Min(n, Math.Max(1, maxCellWidth)))) // each column must be at least 1
                            .ToArray();

        // --- layout math ---------------------------------------------------------
        int colPadding = EffectivePadding(padding, consoleWidth, columnCount);
        int sepWidth = Math.Max(0, (columnCount - 1) * colPadding);
        int budget = consoleWidth - sepWidth;

        // If even 1 char per column doesn't fit, fall back: no padding and drop rightmost columns.
        if (budget < columnCount)
        {
            colPadding = 0;
            sepWidth = 0;
            budget = consoleWidth;
            int keep = Math.Max(1, Math.Min(columnCount, budget)); // at least one column
            if (keep < columnCount)
            {
                headers = headers.Take(keep).ToList();
                bodyRows = bodyRows.Select(r => r.Take(keep).ToArray()).ToList();
                columnCount = keep;
                natural = natural.Take(keep).ToArray();
                desired = desired.Take(keep).ToArray();
            }
        }

        // --- round-robin width distribution (one unit at a time) -----------------
        int[] colWidths = Enumerable.Repeat(1, columnCount).ToArray();
        int remaining = budget - columnCount;

        // Keep handing out single-character "units" to columns that still need them.
        while (remaining > 0)
        {
            bool grewInPass = false;
            for (int i = 0; i < columnCount && remaining > 0; i++)
            {
                if (colWidths[i] < desired[i])
                {
                    colWidths[i]++;
                    remaining--;
                    grewInPass = true;
                }
            }
            if (!grewInPass) break; // all columns satisfied; leave any leftover as trailing fill
        }

        // trailing fill only happens if all columns have reached desired and we still have room
        int bodyWidth = colWidths.Sum();
        int renderedWidth = bodyWidth + sepWidth;
        int trailingFill = Math.Max(0, consoleWidth - renderedWidth);

        string colSep = new string(' ', colPadding);
        string dashSep = new string('-', colPadding);

        // --- render --------------------------------------------------------------

        // Header
        var headerLine = string.Join(colSep, headers.Select((h, i) => Truncate(h ?? "", colWidths[i]).PadRight(colWidths[i])))
                        + new string(' ', trailingFill);
        output.Add(new Components.ColoredRow(headerLine, ConsoleColor.DarkGray));

        // Separator
        var separatorLine = string.Join(dashSep, colWidths.Select(w => new string('-', w)))
                            + new string(' ', trailingFill);
        output.Add(new Components.ColoredRow(separatorLine, ConsoleColor.DarkGray));

        // Body
        foreach (var row in bodyRows)
        {
            // split cells into lines and truncate/pad each visual line to the column width
            var splitCells = Enumerable.Range(0, columnCount)
                .Select(i =>
                {
                    string cell = i < row.Length ? row[i] ?? "" : "";
                    return cell.Split('\n')
                            .Select(line => Truncate(line, colWidths[i]).PadRight(colWidths[i]))
                            .ToList();
                })
                .ToList();

            int maxLines = splitCells.Max(lines => lines.Count);

            // normalize column heights
            for (int i = 0; i < splitCells.Count; i++)
            {
                while (splitCells[i].Count < maxLines)
                    splitCells[i].Add(new string(' ', colWidths[i]));
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

    public List<Components.ColoredRow> RenderLibrariesTable(List<Library> libraries, int consoleWidth)
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

        var table = RenderTable(consoleWidth, headers, rows, "...", 1000, 3);

        return table;
    }

    public List<Components.ColoredRow> RenderDeskItemsTable(Desk desk, int consoleWidth)
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
        return RenderTable(consoleWidth, tableHeaders, rows, "...", 1000, 3);
    }
    string RenderProgress(BorrowedItem.ProgressVariable progress)
    {
        if (string.IsNullOrEmpty(progress.CurrentValue) && string.IsNullOrEmpty(progress.CompletionValue))
            return "";

        return $"{progress.CurrentValue}/{progress.CompletionValue} {progress.Unit}";
    }

    public List<Components.ColoredRow> RenderNoteTable(List<Note> notes, string itemName, int itemId, string libName, int consoleWidth)
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

        return RenderTable(consoleWidth, tableHeaders, rows, "...", 1000, 3);
    }

    public List<Components.ColoredRow> RenderItemsTable(
    List<LibraryItem> items, Library lib, int consoleWidth)
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

        return RenderTable(consoleWidth, tableHeaders, rows, "...", 1000, 3);
    }

    public List<Components.ColoredRow> RenderDesksTable(List<Desk> desks, int consoleWidth)
    {
        var tableHeaders = new[] { "ID", "NAME", "BORROWED ITEMS" }.ToList().TranslateToProfile();

        var rows = desks.Select(d => new[]
                                {
                                d.Id.ToString(),
                                d.Name ?? "",
                                d.BorrowedItems.Count.ToString()
                                }).ToList();

        return RenderTable(consoleWidth, tableHeaders, rows, "...", 1000, 3);
    }

    public List<Components.ColoredRow> RenderPropertyDefinitionsTable(Library lib, int consoleWidth)
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
        return RenderTable(consoleWidth, tableHeaders, rows, "...", 1000, 3);
    }

    public List<Components.ColoredRow> RenderItemPropertiesTable(LibraryItem item, Library lib, int consoleWidth)
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
        return RenderTable(consoleWidth, tableHeaders, rows, "...", 1000, 3);
    }
    public List<Components.ColoredRow> RenderLoanHistoryTable(LoanHistory history, LibraryItem item, string libName, int consoleWidth)
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
        return RenderTable(consoleWidth, tableHeaders, rows, "...", 1000, 3);
    }

}