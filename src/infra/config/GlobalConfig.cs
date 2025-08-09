using System.Text;
using Flexlib.Infrastructure.Interop;

namespace Flexlib.Infrastructure.Config;

public static class GlobalConfig
{
    public static int? ConsoleWidth { get; set; }

    public static Result SetEncoding(Encoding encoding)
    {
        // 2. Apply encoding to console I/O
        try
        {
            Console.OutputEncoding = encoding;
            Console.InputEncoding = encoding;
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to set console encoding to '{encoding.EncodingName}': {ex.Message}");
        }

        // 3. Verify encoding was applied
        if (!Console.OutputEncoding.Equals(encoding))
            return Result.Warn($"Console OutputEncoding was not set to '{encoding.EncodingName}'. Current: {Console.OutputEncoding.WebName}");
        if (!Console.InputEncoding.Equals(encoding))
            return Result.Warn($"Console InputEncoding was not set to '{encoding.EncodingName}'. Current: {Console.InputEncoding.WebName}");

        return Result.Success($"Console encoding set to '{encoding.WebName}'.");
    }

}
