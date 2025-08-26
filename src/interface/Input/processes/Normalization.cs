using Flexlib.Interface.Input.Heuristics;

namespace Flexlib.Interface.Input;

public static partial class Input
{
    public static string[] Normalization(string[] input)
    {
        if (input is not string[] args || args.Length == 0)
            return [];

        var normalized = new string[args.Length];

        for (int i = 0; i < args.Length; i++)
        {
            string temp = args[i];

            if (IsCaseInsensitive)
                temp = temp.ToLowerInvariant();

            if (AddressAnalysis.IsFilePath(temp))
                temp = Infer.AbsolutePath(temp);

            normalized[i] = temp.Trim();
        }

        return normalized;
    }


}
