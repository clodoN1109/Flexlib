using Flexlib.Common;

namespace Flexlib.Interface.Input;

public static class Normalizer
{
    public static string[] Normalize(string[] args)
    {
        if (args == null) return Array.Empty<string>();

        var normalized = new string[args.Length];

        for (int i = 0; i < args.Length; i++)
        {
            if ( AddressAnalysis.IsAnyPathType( args[i] ) ) {
                normalized[i] = args[i].Trim();
            }
            else 
            {
                normalized[i] = args[i].ToLowerInvariant().Trim();
            }
        }

        return normalized;
    }
}

