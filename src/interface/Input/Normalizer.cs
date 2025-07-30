using Flexlib.Infrastructure.Interop;
using Flexlib.Interface.Input.Heuristics;

namespace Flexlib.Interface.Input;

public static class Normalizer
{
    public static string[] Normalize(string[] args)
    {

        if (args == null) return Array.Empty<string>();

        var normalized = new string[args.Length];

        for (int i = 0; i < args.Length; i++)
        {
            string temp = args[i];

            if ( IsCaseInsensitive( temp ))
            {
                temp = temp.ToLowerInvariant();
            }

            if ( AddressAnalysis.IsFilePath( temp ) )
            {
                temp = Infer.AbsolutePath( temp ); 
            }

            temp = temp.Trim();

            normalized[i] = temp;

        }
        
        return normalized;
    }

    private static bool IsCaseInsensitive(string address)
    {
        
        return false; 

    }

}

