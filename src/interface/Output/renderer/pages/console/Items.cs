using Flexlib.Domain;
using Flexlib.Infrastructure.Processing;

namespace Flexlib.Interface.Output;

public partial class ConsoleRenderer
{
    public List<Components.ColoredRow> RenderItemsPage(
    List<LibraryItem> items, Library lib, string filterSequence, string sortSequence,
    double localSizeInBytes, List<string> itemNameFilter, int consoleWidth)
    {
        string title = "LIBRARY ITEMS";

        var topInfoRow = new List<Components.ColoredRow>
        {
            new Components.ColoredRow(
                Components.LineFilled(
                    consoleWidth,
                    "left",
                    0,
                    ' ',
                    $"{lib.Name}/{filterSequence}/{string.Join('|', itemNameFilter.Where(n => n != "*").Select(n => n.IsCompound() ? $"'{n}'" : n))}",
                    $"{sortSequence}"
                ),
                ConsoleColor.DarkGray)
        };

        var footerRow = new List<Components.ColoredRow>
        {
            new Components.ColoredRow(""),
            new Components.ColoredRow(new string('░', consoleWidth), ConsoleColor.Gray),
            new Components.ColoredRow(
                Components.LineSpacedBetween(
                    consoleWidth,
                    string.Join("/", lib.LayoutSequence.Select(p => p.Name)),
                    $"{items.Count} items {localSizeInBytes:N2} bytes"
                ),
                ConsoleColor.DarkGray)
        };

        return RenderPage(
            consoleWidth,
            title,
            topInfoRow,
            footerRow,
            RenderItemsTable(items, lib, consoleWidth)
        );
    }
}