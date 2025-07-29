namespace Flexlib.Common;

public class Domain
{
    public List<string> IncludedValues { get; }

    public Domain(params string[] values)
    {
        IncludedValues = new List<string>(values);
    }

    public bool Contains(string value) => IncludedValues.Contains(value);
}


