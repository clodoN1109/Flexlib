using Flexlib.Domain;

namespace Flexlib.Interface.Output;

public partial class ConsoleRenderer
{

    public List<Components.ColoredRow> RenderPropertyDefinitionsPage(Library lib, int consoleWidth)
    {
        string title = "PROPERTY DEFINITIONS";

        var footerRow = new List<Components.ColoredRow>
        {
            new Components.ColoredRow(""),
            new Components.ColoredRow(new string('░', consoleWidth), ConsoleColor.Gray),
            new Components.ColoredRow($"{lib.PropertyDefinitions.Count} properties")
        };

        return RenderPage(
            consoleWidth,
            title,
            new[] { new Components.ColoredRow(lib.Name ?? "") }.ToList(),
            footerRow,
            RenderPropertyDefinitionsTable(lib, consoleWidth)
        );
    }
}