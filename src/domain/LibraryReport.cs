using Flexlib.Domain;

public class LibraryReport
{
    public string LibraryName { get; }
    public int    NumberOfItems { get; }

    public LibraryReport(Library library)
    {
        LibraryName = library.Name ?? "";
        NumberOfItems = library.Items.Count();
    }    

}