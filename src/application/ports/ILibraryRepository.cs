namespace Flexlib.Application.Ports;

using Flexlib.Domain;

public interface ILibraryRepository
{
    void Save(Library lib);
    bool Exists(string name, string path);
    Library? GetByName(string name);
    IEnumerable<Library> GetAll();
}
