using Flexlib.Infrastructure.Processing;

namespace Flexlib.Interface.Input;

public static class InputPreProcessing
{
    public static void Execute(object input, out PreProcessingResult preProcessed)
    {
        var raw = new ProcessNode<object, object>
        {
            OriginalInput = input,
            LastValue = input,
            NewValue = input
        };

        var process = new Process(raw) { }
            .Apply<object? , string[]>    (Input.Casting)
            .Apply<string[], string[]>    (Input.Sanitization)
            .Apply<string[], string[]>    (Input.Normalization)
            .Apply<string[], ParsedInput> (Input.Parsing);

        var final = process.CurrentValue<ProcessedInput>();

        preProcessed = new PreProcessingResult
        {
            Value = final,
            Original = process.OriginalInput,
            IsValid = final is ParsedInput parsed
        };
    }
}

public class PreProcessingResult
{
    public object? Value { get; set; }
    public object? Original { get; set; }
    public bool IsValid { get; set; }
}

public abstract class ProcessedInput
{
    public abstract bool IsValid();
}


