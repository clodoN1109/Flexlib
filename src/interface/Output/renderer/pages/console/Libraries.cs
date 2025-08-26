using Flexlib.Domain;

namespace Flexlib.Interface.Output;

public partial class ConsoleRenderer
{
    public List<Components.ColoredRow> RenderLibrariesPage(List<Library> libraries, int consoleWidth)
    {
        string title = "LIBRARIES";

        var table = RenderLibrariesTable(libraries, consoleWidth);

        var footerRow = new List<Components.ColoredRow>
        {
            new Components.ColoredRow(""),
            new Components.ColoredRow(new string('░', consoleWidth), ConsoleColor.Gray),
            new Components.ColoredRow($"{libraries.Count} libraries")
        };

        return RenderPage(
            consoleWidth,
            title,
            Enumerable.Empty<Components.ColoredRow>().ToList(),
            footerRow,
            table
        );
    }
}