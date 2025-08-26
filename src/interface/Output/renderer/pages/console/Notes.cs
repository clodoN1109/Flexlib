using Flexlib.Domain;
using Flexlib.Infrastructure.Processing;

namespace Flexlib.Interface.Output;

public partial class ConsoleRenderer
{
    public List<Components.ColoredRow> RenderNotePage(List<Note> notes, string itemName, int itemId, string libName, int consoleWidth)
    {
        string title = "NOTES";

        var topInfoRow = new List<Components.ColoredRow>
        {
            new Components.ColoredRow(
                Components.LineFilled(
                    consoleWidth,
                    "left",
                    0,
                    ' ',
                    $"{libName}/{(itemName.IsCompound() ? $"'{itemName}'" : itemName)} (ID {itemId})"
                )
            )
        };

        var footerRow = new List<Components.ColoredRow>
        {
            new Components.ColoredRow(""),
            new Components.ColoredRow(new string('░', consoleWidth), ConsoleColor.Gray),
            new Components.ColoredRow($"{notes.Count} notes")
        };

        return RenderPage(
            consoleWidth,
            title,
            topInfoRow,
            footerRow,
            RenderNoteTable(notes, itemName, itemId, libName, consoleWidth)

        );
    }
}