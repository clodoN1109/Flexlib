using Flexlib.Infrastructure.Config;

namespace Flexlib.Interface.Input;

public static partial class Input
{
    public static string[] Sanitization(string[] input)
    {
        if (input.Length == 0)
            return [];
        return StripWidthOption(input);
    }

    private static string[] StripWidthOption(string[] input)
    {
        var remaining = new List<string>();
        foreach (var arg in input)
        {
            if (arg.StartsWith("--width=", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(arg.Substring(8), out int width))
                {
                    GlobalConfig.ConsoleWidth = width;
                }
            }
            else
            {
                remaining.Add(arg);
            }
        }
        return remaining.ToArray();
    }
}
