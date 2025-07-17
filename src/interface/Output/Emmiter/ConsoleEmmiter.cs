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

    public void PrintLines(List<string> lines, ConsoleColor? color = null)
    {
        foreach (var line in lines)
            Print(line, color);
    }
}

