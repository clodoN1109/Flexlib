using System.Text.RegularExpressions;

namespace Flexlib.Domain;

public class Comment
{
    public string Id { get; }
    public string Text { get; }
    public List<LibraryItemReference> References { get; }
    
    public Comment(string id, string text)
    {
        Id = id;
        Text = text;
        References = GetInTextReferences(text); 
    }
    

    private List<LibraryItemReference> GetInTextReferences(string text)
    {
        var references = new List<LibraryItemReference>();

        // Updated pattern: optionally matches {lib/item} OR {item}
        var pattern = @"\{(?:(?<lib>[^/\s{}]+)/)?(?<item>[^{}\s]+)\}";
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
