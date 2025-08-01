using Flexlib.Domain;
using Flexlib.Interface.Output;
using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Authentication;
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

        var process = new Process(raw)
            .Apply<object, Normalized>(Input.Normalize)
            .Apply<Normalized, ParsedInput>(Input.Parse);

        var final = process.CurrentValue<ProcessedInput>();

        preProcessed = new PreProcessingResult
        {
            Value = final,
            Original = process.OriginalInput,
            IsValid = final is ParsedInput
        };
    }
}

public class PreProcessingResult
{
    public object? Value { get; set; }
    public object? Original { get; set; }
    public bool? IsValid { get; set; }
}

public abstract class ProcessedInput
{
    public abstract bool IsValid();
}


