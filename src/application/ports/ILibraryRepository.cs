namespace Flexlib.Application.Ports;

using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;

public interface ILibraryRepository
{
    Result Save(Library lib);
    Result Save(LibraryItem item, Library lib);
    bool Exists(string name);
    Library? GetByName(string name);
    Result RemoveLibraryByName(string name);
    Result RemoveItem(LibraryItem item, Library lib);
    IEnumerable<Library> GetAll();
    string GetDataDirectory();
    double GetLocalItemFileSizes(List<LibraryItem> items, Library lib);
}
