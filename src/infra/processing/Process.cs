namespace Flexlib.Infrastructure.Processing;

public class Process
{
    public object OriginalInput { get; }
    private object _currentValue;
    private readonly List<object> _history = new();

    public Process(object processNode)
    {
        if (processNode is not IProcessNode typed)
            throw new ArgumentException("Invalid input: must implement IProcessNode");

        OriginalInput = typed.OriginalInput;
        _currentValue = typed.GetNewValue();
        _history.Add(processNode);
    }

    public IReadOnlyList<object> History => _history;

    public TCurrent CurrentValue<TCurrent>() => (TCurrent)_currentValue;

    public Process Apply<TIn, TOut>(Func<TIn, TOut> transformation)
    {
        var last = (TIn)_currentValue!;
        var result = transformation(last);

        var step = new ProcessNode<TIn, TOut>
        {
            OriginalInput = OriginalInput,
            LastValue = last,
            NewValue = result
        };

        _currentValue = result!;
        _history.Add(step);
        return this;
    }
}

public class ProcessNode<TIn, TOut> : IProcessNode
{
    public required object OriginalInput { get; init; }
    public required TIn LastValue { get; init; }
    public required TOut NewValue { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public object GetNewValue() => NewValue!;
}






