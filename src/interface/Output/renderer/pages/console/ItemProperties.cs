using Flexlib.Domain;
using Flexlib.Infrastructure.Processing;

namespace Flexlib.Interface.Output;

public partial class ConsoleRenderer
{
    public List<Components.ColoredRow> RenderItemPropertiesPage(LibraryItem item, Library lib, int consoleWidth)
    {
        string title = "ITEM PROPERTIES";

        var topInfoRow = new List<Components.ColoredRow>
        {
            new Components.ColoredRow($"{lib.Name}/{(item.Name!.IsCompound() ? $"'{item.Name}'" : item.Name!)}")
        };

        var footerRow = new List<Components.ColoredRow>
        {
            new Components.ColoredRow(""),
            new Components.ColoredRow(new string('░', consoleWidth), ConsoleColor.Gray),
            new Components.ColoredRow($"Item ID: {item.Id} • {lib.PropertyDefinitions.Count} properties")
        };
        return RenderPage(
            consoleWidth,
            title,
            topInfoRow,
            footerRow,
            RenderItemPropertiesTable(item, lib, consoleWidth)
        );
    }
}