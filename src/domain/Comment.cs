using System.Text.RegularExpressions;
using Flexlib.Application.Ports;

namespace Flexlib.Domain;

public class Note
{
    private string _text;

    public string Id { get; init; } = "";
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            References = ExtractReferencesFromText(_text);
            EditedTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        }
    }

    public User Author { get; init; } = new User("Anonymous");
    public string CreatedTime { get; init; } = "";
    public string EditedTime { get; set; } = "";

    public List<LibraryItemReference> References { get; private set; } = new();

    public Note() 
    {
        _text = "";
    }

    public Note(string id, string text, IUser author)
    {
        Id = id;
        _text = text;
        Author = new User(author.Id)
        {
            Name = author.Name,
            Credentials = author.Credentials
        };
        CreatedTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        EditedTime = "";
        References = ExtractReferencesFromText(_text);
    }

    private List<LibraryItemReference> ExtractReferencesFromText(string text)
    {
        var references = new List<LibraryItemReference>();
        var pattern = @"\{(?:(?<lib>[^/{}]+?)/)?(?<item>[^{}]+?)\}";
        var matches = Regex.Matches(text, pattern);

        foreach (Match match in matches)
        {
            if (match.Success)
            {
                string item = match.Groups["item"].Value;
                string lib = match.Groups["lib"].Success && !string.IsNullOrWhiteSpace(match.Groups["lib"].Value)
                    ? match.Groups["lib"].Value
                    : "Default Library";

                references.Add(new LibraryItemReference
                {
                    LibraryName = lib,
                    ItemName = item
                });
            }
        }

        return references;
    }
}


