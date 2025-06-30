namespace Flexlib.Domain;

public class Library
{
    public string Name { get; set; }
    public string Path { get; set; }
    public LibraryItem[] Items { get; }

    public Library(string name, string path)
    {
        Name = name;
        Path = path;
        Items = Array.Empty<LibraryItem>();
    }

}
