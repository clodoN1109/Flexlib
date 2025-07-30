using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases;

public static class NewLibrary
{
    
    public static Result Execute(string name, string path, ILibraryRepository repo)
    {

        Result validation = IsOperationAllowed(name, path, repo);

        if (validation.IsSuccess)
        {
            return NewLib(name, path, repo);
           
        }
        else 
        {
            return validation;
        }
                
    }

    private static Result NewLib(string name, string path, ILibraryRepository repo)
    {
        if ( string.IsNullOrWhiteSpace(path) )
            path = repo.GetDataDirectory();

        var lib = new Library(name, path);
        repo.Save(lib);
        
        return Result.Success($"Library {name} created in {path}.");
    }

    private static Result IsOperationAllowed(string name, string path, ILibraryRepository repo)
    {
        if (repo.Exists(name))
            return Result.Fail("Library already exists.");
        else 
            return Result.Success("Operation allowed. Library can be created.");

    }

}
