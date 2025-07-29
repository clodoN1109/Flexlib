namespace Flexlib.Common;

public class VariableDomain
{
    public List<string> IncludedValues { get; }

    public VariableDomain(params string[] values)
    {
        IncludedValues = new List<string>(values);
    }

    public VariableDomain(List<string> values)
    {
        IncludedValues = values;
    }
    
    public bool Contains(string value) => IncludedValues.Contains(value);
}


