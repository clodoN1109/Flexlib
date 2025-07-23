namespace Flexlib.Application.Ports;

using Flexlib.Domain;
using Flexlib.Common;

public interface ILibraryRepository
{
    void Save(Library lib);
    bool Exists(string name);
    Library? GetByName(string name);
    Result RemoveLibraryByName(string name);
    Result RemoveItem(LibraryItem item, Library lib);
    IEnumerable<Library> GetAll();
    string GetDataDirectory();
}
