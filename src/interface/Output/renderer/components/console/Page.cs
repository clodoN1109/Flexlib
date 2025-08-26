namespace Flexlib.Interface.Output;
public partial class ConsoleRenderer
{
    public static List<Components.ColoredRow> RenderPage(
    int consoleWidth,
    string title,
    List<Components.ColoredRow> topInfoRow,
    List<Components.ColoredRow> footerRow,
    List<Components.ColoredRow> bodyRows
)
    {
        var output = new List<Components.ColoredRow>
        {
            new Components.ColoredRow(""),
            new Components.ColoredRow(Components.LineFilled(consoleWidth, "left", 5, '░', title), ConsoleColor.DarkGray),
            new Components.ColoredRow("")
        };

        if (topInfoRow != null) output.AddRange(topInfoRow);
        output.Add(new Components.ColoredRow(""));

        if (bodyRows != null) output.AddRange(bodyRows);

        if (footerRow != null) output.AddRange(footerRow);

        return output;
    }
}