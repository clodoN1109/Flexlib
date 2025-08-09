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
using Flexlib.Infrastructure.Environment;

namespace Flexlib.Interface.Output;


public partial class ConsoleRenderer
{

    public Components.WrappedMessage Message(string? message, ConsoleColor color = ConsoleColor.Gray)
    {
        var lines = new List<Components.ColoredLine>();

        if (string.IsNullOrWhiteSpace(message))
            return new Components.WrappedMessage { Lines = lines };

        int maxBoxWidth = Env.GetSafeWindowWidth() - 4;
        int innerWidth = Math.Max(10, maxBoxWidth - 6);

        var wrappedLines = new List<string>();

        foreach (var rawLine in message.Split('\n'))
        {
            var remaining = rawLine.Trim();
            while (remaining.Length > innerWidth)
            {
                wrappedLines.Add("   " + remaining[..innerWidth]);
                remaining = remaining[innerWidth..];
            }

            wrappedLines.Add("   " + remaining);
        }

        int contentWidth = wrappedLines.Max(l => l.Length);
        int boxWidth = contentWidth + 2;

        string top = "---" + new string('―', boxWidth - 3) + "┐";
        string bottom = "---" + new string('―', boxWidth - 3) + "┘";

        lines.Add(new Components.ColoredLine(top, color));

        foreach (var line in wrappedLines)
        {
            string padded = line.PadRight(contentWidth);
            lines.Add(new Components.ColoredLine($" {padded} │", color));
        }

        lines.Add(new Components.ColoredLine(bottom, color));

        return new Components.WrappedMessage { Lines = lines };
    }
    
    public Components.WrappedMessage Success(string message) =>
        Message($"✓ {message}", ConsoleColor.Green);

    public Components.WrappedMessage Warning(string message) =>
        Message($"⚠  {message}", ConsoleColor.Yellow);

    public Components.WrappedMessage Failure(string message) =>
        Message($"✗ {message}", ConsoleColor.Red);

    public Components.WrappedMessage RenderResult(Result result)
    {
        var all = new List<Components.ColoredLine>();

        if (result.IsSuccess)
            all.AddRange(Success(result.SuccessMessage ?? "").Lines);

        if (result.IsWarning)
            all.AddRange(Warning(result.WarningMessage ?? "").Lines);

        if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            all.AddRange(Failure(result.ErrorMessage).Lines);

        return new Components.WrappedMessage { Lines = all };
    }

    public Components.WrappedMessage UserInfo(string userInfo) =>
        Message($"{userInfo} 🪪", ConsoleColor.Cyan);

    public Components.WrappedMessage AuthStatus(string message) =>
        Message($"🪪 {message}", ConsoleColor.Cyan);

    public Components.WrappedMessage Error(string message) =>
        Message($"Error: {message}", ConsoleColor.Red);

}
