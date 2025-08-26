using Flexlib.Infrastructure.Processing;
using Flexlib.Interface.Input;

namespace Flexlib.Interface.Output;


public partial class ConsoleRenderer
{

    public List<Components.ColoredRow> AvailableActions(List<string> actions, int consoleWidth)
    {
        var lines = new List<Components.ColoredRow>();

        if (actions == null || actions.Count == 0)
        {
            lines.Add(new Components.ColoredRow("No available actions.", ConsoleColor.DarkGray));
            return lines;
        }

        const int padding = 2;
        string label = "commands: ";
        var commandLines = new List<string>();

        string currentLine = label;
        int currentWidth = label.Length;

        foreach (var cmd in actions)
        {
            string segment = cmd + new string(' ', padding);

            if (currentWidth + segment.Length > consoleWidth - 4) // account for border + margin
            {
                commandLines.Add(currentLine.TrimEnd());
                currentLine = new string(' ', label.Length) + segment;
                currentWidth = label.Length + segment.Length;
            }
            else
            {
                currentLine += segment;
                currentWidth += segment.Length;
            }
        }

        if (!string.IsNullOrWhiteSpace(currentLine))
        {
            commandLines.Add(currentLine.TrimEnd());
        }

        // Render boxed output
        string borderLine = "---" + new string('―', consoleWidth - 6) + "┐";
        lines.Add(new Components.ColoredRow(borderLine, ConsoleColor.DarkGray));

        foreach (var line in commandLines)
        {
            string padded = "  " + line.PadRight(consoleWidth - 6);
            lines.Add(new Components.ColoredRow(padded + " │", ConsoleColor.Gray));
        }

        string bottomLine = "---" + new string('―', consoleWidth - 6) + "┘";
        lines.Add(new Components.ColoredRow(bottomLine, ConsoleColor.DarkGray));

        return lines;
    }

    public List<Components.ColoredRow> UsageInfo(UsageInfo info, int consoleWidth)
    {
        var lines = new List<Components.ColoredRow>();

        string logo = Components.LogoLine(consoleWidth);
        string paddedTitle = $"{(info is CommandUsageInfo cmdInfo ? cmdInfo.Group.Icon + " " : "")}{info?.Title.ToUpperInvariant()} ";

        // lines.Add(new Components.ColoredRow(""));
        // lines.Add(new Components.ColoredRow(logo));
        lines.Add(new Components.ColoredRow(""));
        lines.Add(new Components.ColoredRow(paddedTitle, ConsoleColor.Gray));

        // Metadata
        if (info?.Meta?.Any() == true)
        {
            lines.Add(new Components.ColoredRow(""));
            lines.Add(new Components.ColoredRow(string.Join("  •  ", info.Meta), ConsoleColor.DarkGray));
        }

        // Description
        if (!string.IsNullOrWhiteSpace(info?.Description))
        {
            lines.Add(new Components.ColoredRow(""));
            var wrapped = Components.WrappedText(info.Description, consoleWidth);
            foreach (var line in wrapped)
                lines.Add(new Components.ColoredRow(line, ConsoleColor.White));
        }

        // Usage Syntax
        if (!string.IsNullOrWhiteSpace(info?.Syntax))
        {
            lines.Add(new Components.ColoredRow(""));
            lines.Add(new Components.ColoredRow("usage:", ConsoleColor.Cyan));
            lines.Add(new Components.ColoredRow(""));
            lines.Add(new Components.ColoredRow("   " + info.Syntax, ConsoleColor.White));
        }

        // Options
        if (info?.Options?.Any() == true)
        {
            lines.Add(new Components.ColoredRow(""));
            lines.Add(new Components.ColoredRow("options:", ConsoleColor.Cyan));
            lines.Add(new Components.ColoredRow(""));

            foreach (var opt in info.Options.OrderByDescending(opt => opt.Mandatory))
            {
                var name = opt.Mandatory ? $"<{opt.Name}>" : $"[{opt.Name}]";
                var domain = opt.OptionDomain?.IncludedValues?.Any() == true
                    ? $" ({string.Join("|", opt.OptionDomain.IncludedValues.OrderBy(v => v))})"
                    : "";
                var defaultVal = !string.IsNullOrWhiteSpace(opt.DefaultValue)
                    ? $" (default: {opt.DefaultValue})"
                    : "";

                var label = $"    {name}{domain}{defaultVal}";
                if (label.Length > consoleWidth/1.5)
                {
                    var domainParts = domain.SplitInParts("|", consoleWidth/3, consoleWidth/2);
                    domain = string.Join("\n\t\t", domainParts);
                    label = $"    {name}{domain}{defaultVal}";
                }
                

                lines.Add(new Components.ColoredRow(label, opt.Mandatory ? ConsoleColor.Yellow : ConsoleColor.DarkGray, false));

                var wrappedDesc = Components.WrappedText(opt.Description, consoleWidth - 6);
                foreach (var descLine in wrappedDesc)
                    lines.Add(new Components.ColoredRow("      " + descLine, ConsoleColor.Gray));
                
                lines.Add(new Components.ColoredRow("      " + opt.Syntax, ConsoleColor.Gray));
            }
        }

        // Examples
        if (info?.Examples.Count > 0 ) 
        {
            lines.Add(new Components.ColoredRow("examples:", ConsoleColor.Cyan));
        }
        foreach (var example in info?.Examples ?? new List<string>())
        {
            if (!string.IsNullOrWhiteSpace(example))
            {
                lines.Add(new Components.ColoredRow("")); // Adds an empty line
                lines.Add(new Components.ColoredRow("   " + example, ConsoleColor.White));
            }
        }

        lines.Add(new Components.ColoredRow(""));

        return lines;
    }
}
