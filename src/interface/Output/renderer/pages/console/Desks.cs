using Flexlib.Domain;
using Flexlib.Infrastructure.Processing;

namespace Flexlib.Interface.Output;

public partial class ConsoleRenderer
{
    public List<Components.ColoredRow> RenderDesksPage(List<Desk> desks, string libraryName, int consoleWidth)
    {
        string title = "DESKS";

        var footerRow = new List<Components.ColoredRow>
        {
            new Components.ColoredRow(""),
            new Components.ColoredRow(new string('░', consoleWidth), ConsoleColor.Gray),
            new Components.ColoredRow($"{desks.Count} desks")
        };

        return RenderPage(
            consoleWidth,
            title,
            new[] { new Components.ColoredRow(libraryName) }.ToList(),
            footerRow,
            RenderDesksTable(desks, consoleWidth)
        );
    }
}