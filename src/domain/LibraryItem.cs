using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using Flexlib.Common;

namespace Flexlib.Domain;

public class LibraryItem
{
    public string? Name { get; set; }
    public string? Origin { get; set; }

    [JsonIgnore]
    public readonly Library? _library;

    public List<Comment> Comments { get; set; }

    public Dictionary<string, object?> PropertyValues { get; set; }

    public LibraryItem(string name, string origin, Library library)
    {
        Name = TextUtil.Truncate(name, 50);
        Origin = origin;
        _library = library;
        Comments = new List<Comment>();
        PropertyValues = new Dictionary<string, object?>();
    }

    public LibraryItem()
    {
        Comments = new List<Comment>();
        PropertyValues = new Dictionary<string, object?>();
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

    public void AddComment(Comment newComment)
    {
        Comments.Add(newComment);
    }

    public Comment? GetCommentById(string id)
    {
        return Comments.FirstOrDefault(c => c.Id == id);
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
}
