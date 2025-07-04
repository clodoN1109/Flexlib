using System.Text.Json.Serialization;

namespace Flexlib.Domain;

public class LibraryItem
{
    
    public string? Name { get; set; }
    public string? Origin { get; set; }
    [JsonIgnore]
    public readonly Library? _library;

    public List<string> Comments { get; }
    
    public Dictionary<string, object?> PropertyValues { get; set; }

    public LibraryItem(string? name, string origin, Library library)
    {
        Name = name;
        Origin = origin;
        _library = library;
        Comments = new();
        PropertyValues = new();
    }

    public LibraryItem() { 

        Comments = new();
        PropertyValues = new();

    }

    public T? GetValue<T>(string propertyName)
    {
        return PropertyValues.TryGetValue(propertyName, out var value) && value is T casted
            ? casted
            : default;
    }

    public void SetValue(string propertyName, string value)
    {

        if ( _library!.isPropertyValueValid(propertyName, value) )
            PropertyValues[propertyName!] = value;
        else
            throw new ArgumentException("Invalid arguments.");
    }

    public Library? GetLibrary()
    {
        return _library;
    }


}
