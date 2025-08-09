using Flexlib.Domain;
using System.Text.Json;
using System.Drawing;
using Flexlib.Application.Ports;
using Flexlib.Interface.CLI;
using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Processing;
using Flexlib.Interface.Output;
using Flexlib.Interface.Input;
using System.Linq;

namespace Flexlib.Interface.Output;


public partial class ConsoleRenderer
{

    public List<Components.ColoredLine> AvailableActions(List<string> actions, int consoleWidth)
    {
        var lines = new List<Components.ColoredLine>();

        if (actions == null || actions.Count == 0)
        {
            lines.Add(new Components.ColoredLine("No available actions.", ConsoleColor.DarkGray));
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
        lines.Add(new Components.ColoredLine(borderLine, ConsoleColor.DarkGray));

        foreach (var line in commandLines)
        {
            string padded = "  " + line.PadRight(consoleWidth - 6);
            lines.Add(new Components.ColoredLine(padded + " │", ConsoleColor.Gray));
        }

        string bottomLine = "---" + new string('―', consoleWidth - 6) + "┘";
        lines.Add(new Components.ColoredLine(bottomLine, ConsoleColor.DarkGray));

        return lines;
    }

    public List<Components.ColoredLine> UsageInfo(UsageInfo info, int consoleWidth)
    {
        var lines = new List<Components.ColoredLine>();

        string separator = new string('░', consoleWidth);
        string logo = Components.LogoLine(consoleWidth);
        string title = $"░░░░ {(info is CommandUsageInfo cmdInfo ? cmdInfo.Group.Icon + " " : "")}{info?.Title.ToUpperInvariant()} ";
        string paddedTitle = title + new string('░', Math.Max(0, consoleWidth - title.Length));

        lines.Add(new Components.ColoredLine(""));
        lines.Add(new Components.ColoredLine(logo));
        lines.Add(new Components.ColoredLine(""));
        lines.Add(new Components.ColoredLine(paddedTitle, ConsoleColor.Gray));

        // Metadata
        if (info?.Meta?.Any() == true)
        {
            lines.Add(new Components.ColoredLine(""));
            lines.Add(new Components.ColoredLine(string.Join("  •  ", info.Meta), ConsoleColor.DarkGray));
        }

        // Description
        if (!string.IsNullOrWhiteSpace(info?.Description))
        {
            lines.Add(new Components.ColoredLine(""));
            var wrapped = Components.WrappedText(info.Description, consoleWidth);
            foreach (var line in wrapped)
                lines.Add(new Components.ColoredLine(line, ConsoleColor.White));
        }

        // Usage Syntax
        if (!string.IsNullOrWhiteSpace(info?.Syntax))
        {
            lines.Add(new Components.ColoredLine(""));
            lines.Add(new Components.ColoredLine("usage:", ConsoleColor.Cyan));
            lines.Add(new Components.ColoredLine(""));
            lines.Add(new Components.ColoredLine("   " + info.Syntax, ConsoleColor.White));
        }

        // Options
        if (info?.Options?.Any() == true)
        {
            lines.Add(new Components.ColoredLine(""));
            lines.Add(new Components.ColoredLine("options:", ConsoleColor.Cyan));
            lines.Add(new Components.ColoredLine(""));

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
                lines.Add(new Components.ColoredLine(label, opt.Mandatory ? ConsoleColor.Yellow : ConsoleColor.DarkGray, false));

                var wrappedDesc = Components.WrappedText(opt.Description, consoleWidth - 6);
                foreach (var descLine in wrappedDesc)
                    lines.Add(new Components.ColoredLine("      " + descLine, ConsoleColor.Gray));
                
                lines.Add(new Components.ColoredLine("      " + opt.Syntax, ConsoleColor.Gray));
            }
        }

        // Examples
        if (info?.Examples.Count > 0 ) 
        {
            lines.Add(new Components.ColoredLine("examples:", ConsoleColor.Cyan));
        }
        foreach (var example in info?.Examples ?? new List<string>())
        {
            if (!string.IsNullOrWhiteSpace(example))
            {
                lines.Add(new Components.ColoredLine("")); // Adds an empty line
                lines.Add(new Components.ColoredLine("   " + example, ConsoleColor.White));
            }
        }

        lines.Add(new Components.ColoredLine(""));
        lines.Add(new Components.ColoredLine(separator, ConsoleColor.Gray));
        lines.Add(new Components.ColoredLine(""));

        return lines;
    }
}
