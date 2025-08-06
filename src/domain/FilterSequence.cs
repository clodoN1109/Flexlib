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
        input = input.Trim();

        if (string.IsNullOrWhiteSpace(input))
            return new List<string>();

        if (input.Contains('-'))
        {
            var parts = input.Split('-', 2, StringSplitOptions.RemoveEmptyEntries)
                             .Select(p => p.Trim())
                             .ToArray();

            if (parts.Length == 2)
            {
                var startStr = parts[0];
                var endStr = parts[1];

                if (int.TryParse(startStr, out int startInt) && int.TryParse(endStr, out int endInt))
                {
                    // Integer range
                    return Enumerable.Range(startInt, endInt - startInt + 1)
                                     .Select(i => i.ToString())
                                     .ToList();
                }

                if (decimal.TryParse(startStr, out decimal startDec) && decimal.TryParse(endStr, out decimal endDec))
                {
                    // Determine step based on decimal precision of the inputs
                    int GetDecimalPlaces(string s)
                    {
                        var index = s.IndexOf('.');
                        return index < 0 ? 0 : s.Length - index - 1;
                    }

                    int precision = Math.Max(GetDecimalPlaces(startStr), GetDecimalPlaces(endStr));
                    decimal step = (decimal)Math.Pow(10, -precision);

                    var result = new List<string>();
                    for (decimal d = startDec; d <= endDec; d += step)
                    {
                        result.Add(d.ToString($"F{precision}").TrimEnd('0').TrimEnd('.'));
                    }
                    return result;
                }

                return new List<string> { $"{startStr}-{endStr}" };
            }
        }

        return input
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }
}

