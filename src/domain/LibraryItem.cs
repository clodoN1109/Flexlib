using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Flexlib.Infrastructure.Processing;

namespace Flexlib.Domain;

public class LibraryItem
{
    public string? Name { get; set; }
    public string? Origin { get; set; }
    public int Id { get; set; }
    public string? FileExtension { get; set; }

    [JsonIgnore]
    public readonly Library? _library;

    public List<Comment> Comments { get; set; }

    public Dictionary<string, object?> PropertyValues { get; set; }

    public LibraryItem(string name, string origin, Library library)
    {
        Name = TextUtil.Truncate(name, 50);
        Origin = origin;
        Id = library.GetHighestItemId() + 1;
        if (Origin != null)
            FileExtension = Path.GetExtension(Origin) ?? "";

        _library = library;
        Comments = new List<Comment>();
        PropertyValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    public LibraryItem()
    {
        Comments = new List<Comment>();
        PropertyValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    public void RemoveComment(string commentID)
    {
        Comments.RemoveAll(c => c.Id.ToLowerInvariant() == commentID.ToLowerInvariant());
    }
        
    public T? GetValue<T>(string propertyName)
    {
        return PropertyValues.TryGetValue(propertyName, out var value) && value is T casted
            ? casted
            : default;
    }

    public void SetValue(string propertyName, string value)
    {
        if (_library != null && _library.isPropertyValueValid(propertyName, value))
            PropertyValues[propertyName] = value;
        else
            throw new ArgumentException("Invalid arguments.");
    }

    public Library? GetLibrary()
    {
        return _library;
    }

    public void NewComment(Comment newComment)
    {
        Comments.Add(newComment);
    }

    public Comment? GetCommentById(string id)
    {
        return Comments.FirstOrDefault(c => c.Id.ToLowerInvariant() == id.ToLowerInvariant());
    }

    public int GetCommentCount()
    {
        return Comments.Count;
    }

    public LibraryItemReference GetReference()
    {
        return new LibraryItemReference
        {
            ItemName = Name ?? "",
            LibraryName = _library?.Name ?? ""
        };
    }
   
    public Dictionary<string, List<string>> GetPropertyValuesAsListOfStrings()
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        if (PropertyValues == null)
            return result;

        foreach (var (key, value) in PropertyValues)
        {
            if (value == null)
            {
                result[key] = new List<string>();
            }
            else if (value is string s)
            {
                result[key] = new List<string> { s };
            }
            else if (value is IEnumerable<string> stringList)
            {
                result[key] = stringList.ToList();
            }
            else if (value is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.String)
                {
                    result[key] = new List<string> { je.GetString() ?? "" };
                }
                else if (je.ValueKind == JsonValueKind.Array)
                {
                    var strings = je.EnumerateArray()
                        .Where(e => e.ValueKind == JsonValueKind.String)
                        .Select(e => e.GetString() ?? "")
                        .ToList();

                    result[key] = strings;
                }
                else
                {
                    result[key] = new List<string> { je.ToString() };
                }
            }
            else
            {
                result[key] = new List<string> { value.ToString() ?? "" };
            }
        }

        return result;
    }

}
