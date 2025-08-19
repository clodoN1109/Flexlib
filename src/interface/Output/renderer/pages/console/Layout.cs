namespace Flexlib.Interface.Output;
public partial class ConsoleRenderer
{
    public List<string> RenderLayoutSequence(List<string> layoutSequence)
    {
        var lines = new List<string>();

        if (layoutSequence.Count == 0)
        {
            lines.Add(string.Empty);
            lines.Add("Layout sequence is empty.");
            return lines;
        }

        lines.Add(string.Empty);
        lines.Add("layout structure:");
        lines.Add(string.Empty);
        lines.Add("📂");

        string indentUnit = "  ";
        for (int i = 0; i < layoutSequence.Count; i++)
        {
            string indent = string.Concat(Enumerable.Repeat(indentUnit, i));
            string symbol = i == layoutSequence.Count - 1 ? "└─" : "├─";
            lines.Add($"{indent}{symbol} {layoutSequence[i]}");
        }

        lines.Add(string.Empty);
        return lines;
    }
}