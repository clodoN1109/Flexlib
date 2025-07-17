namespace Flexlib.Domain;

public class FilterSequence
{
    public List<List<string>> Elements { get; } = new();

    public FilterSequence(string filterSequence)
    {
        var rawElements = filterSequence.Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach (var element in rawElements)
        {
            var values = ParseValues(element.Trim());
            if (values.Count > 0)
            {
                Elements.Add(values);
            }
        }
    }

    private List<string> ParseValues(string input)
    {
        if (input.Contains('-'))
        {
            var parts = input.Split('-', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                return new List<string> { parts[0].Trim(), parts[1].Trim() };
            }
        }

        return input
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }
}

