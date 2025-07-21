using System.Text.RegularExpressions;

namespace Flexlib.Domain;

public class Comment
{
    private string _text;

    public string Id { get; }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            References = ExtractReferencesFromText(_text);
        }
    }

    public List<LibraryItemReference> References { get; private set; }

    public Comment(string id, string text)
    {
        Id = id;
        _text = text;
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

