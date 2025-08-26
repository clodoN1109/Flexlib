using System.Drawing;
using Flexlib.Domain;
using Flexlib.Infrastructure.Processing;


namespace Flexlib.Interface.Output;


public partial class ConsoleRenderer
{

    public List<Components.ColoredRow> RenderLoanHistoryPage(LoanHistory history, LibraryItem item, string libName, int consoleWidth)
    {
        string title = "LOAN HISTORY";

        var topInfoRow = new List<Components.ColoredRow>
        {
            new Components.ColoredRow(
                Components.LineFilled(consoleWidth, "left", 0, ' ',
                    $"{libName}/{item.Name ?? $"#{item.Id}"}"))
        };

        var footerRow = new List<Components.ColoredRow>
        {
            new Components.ColoredRow(""),
            new Components.ColoredRow(new string('░', consoleWidth), ConsoleColor.Gray),
            new Components.ColoredRow($"{history.Entries.Count} entr{(history.Entries.Count == 1 ? "y" : "ies")}")
        };

        return RenderPage(
            consoleWidth,
            title,
            topInfoRow,
            footerRow,
            RenderLoanHistoryTable(history, item, libName, consoleWidth)
        );
    }

}