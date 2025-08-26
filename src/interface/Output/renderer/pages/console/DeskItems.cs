using Flexlib.Domain;
using Flexlib.Infrastructure.Processing;

namespace Flexlib.Interface.Output;

public partial class ConsoleRenderer
{
    public List<Components.ColoredRow> RenderDeskItemsPage(Desk desk, string libName, int consoleWidth)
    {
        string title = "DESK ITEMS";
        var topInfoRow = new List<Components.ColoredRow>
        {
            new Components.ColoredRow(
                Components.LineFilled(
                    consoleWidth,
                    "left",
                    0,
                    ' ',
                    $"{libName}/{(desk.Name!.IsCompound() ? $"'{desk.Name}'" : desk.Name)}"
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
                    $"Desk ID: {desk.Id}",
                    $"{desk.BorrowedItems.Count} items"
                ),
                ConsoleColor.DarkGray)
        };

        return RenderPage(
            consoleWidth,
            title,
            topInfoRow,
            footerRow,
            RenderDeskItemsTable(desk, consoleWidth)
        );


    }

}