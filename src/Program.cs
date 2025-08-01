using Flexlib.Interface.Input;
using Flexlib.Interface.Router;
using Flexlib.Interface.Output;
using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Processing;


class Program 
{
    private static readonly AgnosticEmitter _emitter = new();

    static void Main(string[] input)
    {
        if (Initialize(out var result).IsFailure)
        {
            _emitter.Emit(result.ErrorMessage!);
            return;
        }

        InputPreProcessing.Execute(input, out PreProcessingResult preprocessing);

        if (preprocessing.IsValid == true && preprocessing.Value is ProcessedInput validInput)
        {
            Router.Route(validInput);
        }
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

