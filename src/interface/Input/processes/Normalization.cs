using Flexlib.Infrastructure.Interop;
using Flexlib.Interface.Input.Heuristics;

namespace Flexlib.Interface.Input;

public static partial class Input
{
    public static Normalized Normalize(object input)
    {
        if (input is not string[] args || args.Length == 0)
            return new Normalized(Array.Empty<string>());

        var normalized = new string[args.Length];

        for (int i = 0; i < args.Length; i++)
        {
            string temp = args[i];

            if (IsCaseInsensitive(temp))
                temp = temp.ToLowerInvariant();

            if (AddressAnalysis.IsFilePath(temp))
                temp = Infer.AbsolutePath(temp);

            normalized[i] = temp.Trim();
        }

        return new Normalized(normalized);
    }

    private static bool IsCaseInsensitive(string s) => false;

}

public class Normalized : ProcessedInput
{
    public string[] Args { get; }

    public Normalized(string[] args)
    {
        Args = args;
    }

    public override bool IsValid() => Args.Length > 0;
}
