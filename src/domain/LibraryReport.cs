using Flexlib.Domain;

public class LibraryReport
{
    public string Title { get; }
    public int TotalCount { get; }

    // For each property definition → value counts
    public Dictionary<string, Dictionary<string, int>> Properties { get; }

    public LibraryReport(Library library)
    {
        Title = library.Name ?? "";
        TotalCount = library.Items.Count;
        Properties = new();

        foreach (var def in library.PropertyDefinitions)
        {
            // Skip numeric and list properties for now
            if (def.TypeName is "integer" or "decimal" or "float" or "list")
                continue;

            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in library.Items)
            {
                if (item.PropertyValues.TryGetValue(def.Name, out var value) && value is not null)
                {
                    var key = value.ToString() ?? "";

                    if (counts.ContainsKey(key))
                        counts[key]++;
                    else
                        counts[key] = 1;
                }
            }

            Properties[def.Name] = counts;
        }
    }
}
