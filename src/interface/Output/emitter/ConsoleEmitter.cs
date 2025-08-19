using System;
using System.Diagnostics;
using Flexlib.Infrastructure.Config;
using Flexlib.Infrastructure.Environment;
using Flexlib.Interface.Output;

namespace Flexlib.Interface.Output;

public class ConsoleEmitter
{
    public void Print(string message, ConsoleColor? color = null)
    {
        if (color.HasValue)
            Console.ForegroundColor = color.Value;

        Console.WriteLine(message);

        if (color.HasValue)
            Console.ResetColor();
    }

    public void PrintLines(List<Components.ColoredLine> lines, bool clearHost = true)
    {

        if (clearHost)
        {
            bool isInteractiveTerminal = !Console.IsOutputRedirected && !Console.IsErrorRedirected;
            if (isInteractiveTerminal)
            {
                Console.Clear();
            }
        }

        foreach (var line in lines)
            Print(line.Text, line.Color);
    }
    public void PrintLines(List<string> lines, ConsoleColor color, bool clearHost = true)
    {
        if (clearHost) {
            bool isInteractiveTerminal = !Console.IsOutputRedirected && !Console.IsErrorRedirected;
            if (isInteractiveTerminal) {
                Console.Clear();
            }
        }

        foreach (var line in lines)
            Print(line, color);
    }
}

public static class PrettyException
{
#if DEBUG
    public static void HookGlobalHandler()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                PrintFormatted(ex);
            }
            else
            {
                Console.WriteLine("Unhandled exception occurred.");
            }

            Environment.Exit(1);
        };
    }

    private static void PrintFormatted(Exception ex)
    {
        var consoleWidth = Env.GetSafeWindowWidth();
        var divider = new string('░', consoleWidth);
        var logBar = $"░░░░ EXCEPTION LOG ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░";

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine();
        Console.WriteLine(logBar.PadRight(consoleWidth, '░'));
        Console.ResetColor();
        Console.WriteLine();

        var trace = new StackTrace(ex, true);
        var frames = trace.GetFrames();

        bool isFirst = true;
        string frameDivisor;
        if (frames != null && frames.Length > 0)
        {
            foreach (var frame in frames)
            {
                if (isFirst) {
                    frameDivisor ="╭─▶ 💥 " + $"{ex.GetType().Name}: {ex.Message}" ; 
                    isFirst = false;
                }
                else {
                    frameDivisor ="▲ ";
                }

                Console.WriteLine(frameDivisor);
                
                var method = frame.GetMethod();
                var methodName = method != null
                    ? $"{method.DeclaringType?.FullName}.{method.Name}"
                    : "UnknownMethod";

                var filePath = frame.GetFileName();
                var line = frame.GetFileLineNumber();

                // Print method (green)
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"│ {methodName}");
                Console.ResetColor();

                // Print path + line (yellow), or fallback
                if (!string.IsNullOrEmpty(filePath))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"╰─ {filePath}:{line}");
                    Console.ResetColor();
                }
                
            }
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine($"[ERROR] {ex.GetType().Name}: {ex.Message}");
        }

        Console.ForegroundColor = ConsoleColor.DarkRed;

        Console.WriteLine(divider);

        Console.ResetColor();

        Console.WriteLine();
    }
#endif
}

