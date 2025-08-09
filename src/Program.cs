using Flexlib.Interface.Input;
using Flexlib.Interface.Router;
using Flexlib.Interface.Output;
using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Processing;
using Flexlib.Infrastructure.Config;


public class Program
{
    private static readonly AgnosticEmitter _emitter = new();
    public static int windowWidth;

    public static void Main(string[] input)
    {
        // Extract and remove --width=<value>
        input = StripWidthOption(input);

        if (Initialize(out var result).IsFailure)
        {
            _emitter.Emit(result.ErrorMessage!);
            return;
        }

        InputPreProcessing.Execute(input, out PreProcessingResult processed);

        if (processed.IsValid)
        {
            Router.Route((ProcessedInput)processed.Value!);
        }
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

    static Result Initialize(out Result result)
    {
        try
        {
#if DEBUG
            PrettyException.HookGlobalHandler();
#endif
            result = Result.Success("");
            return result;
        }
        catch (Exception ex)
        {
            result = Result.Fail($"Initialization failed: {ex.Message}");
            return result;
        }
    }
}

