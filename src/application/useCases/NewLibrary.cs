using Flexlib.Domain;
using Flexlib.Application.Common;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases;

public static class NewLibrary
{
    
    public static Result Execute(string name, string path, ILibraryRepository repo)
    {
        if (repo.Exists(name, path))
            return Result.Fail("Library already exists.");

        var lib = new Library(name, path);
        repo.Save(lib);
        return Result.Success("Library created!");
    }

}
