namespace Flexlib.Application.Ports;

using Flexlib.Domain;

public interface ILibraryRepository
{
    void Save(Library lib);
    bool Exists(string name);
    Library? GetByName(string name);
    void RemoveLibraryByName(string name);
    IEnumerable<Library> GetAll();
}
