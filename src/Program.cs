using Flexlib.Interface.Input;
using Flexlib.Interface.Router;
using Flexlib.Interface.Output;
using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Processing;
using Flexlib.Infrastructure.Config;
using System.Text;


public class Program
{
    private static readonly AgnosticEmitter _emitter = new();
    public static int windowWidth;

    public static void Main(string[] input)
    {

        if (Initialization(out var result).IsFailure)
        {
            _emitter.Emit(result.ErrorMessage!);
            return;
        }

        InputPreProcessing.Execute(input, out PreProcessingResult output);

        if (output.IsValid)
        {
            Router.Route((ProcessedInput)output.Value!);
        }
    }

    static Result Initialization(out Result result)
    {
        try
        {
            GlobalConfig.SetEncoding(Encoding.UTF8);
#if DEBUG
            PrettyException.HookGlobalHandler();
#endif
            result = Result.Success("Program initialized.");
            return result;
        }
        catch (Exception ex)
        {
            result = Result.Fail($"Initialization failed: {ex.Message}");
            return result;
        }
    }
}

