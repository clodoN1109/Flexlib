namespace Flexlib.Domain;

public class LibraryItem
{
    
    public string Name { get; set; }
    public string Path { get; set; }
    public string[] Comments { get; }

    public LibraryItem(string name, string path)
    {
        Name = name;
        Path = path;
        Comments = Array.Empty<string>();

    }
}
