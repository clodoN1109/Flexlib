using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases;

public static class RemoveLibrary
{
    
    public static Result Execute(string name, ILibraryRepository repo)
    {

        Result validation = IsOperationAllowed(name, repo);

        if (validation.IsSuccess)
        {
            return _RemoveLibrary(name, repo);
        }
        else 
        {
            return validation;
        }
                
    }

    private static Result _RemoveLibrary(string libName, ILibraryRepository repo)
    {
        Library? lib = repo.GetByName(libName);
        if (lib != null)
        {
            return repo.RemoveLibraryByName(libName); 
        }
        else {
            return Result.Fail($"Library '{libName}' not found.");
        }
        
    }

    private static Result IsOperationAllowed(string name, ILibraryRepository repo)
    {
        if (!repo.Exists(name))
            return Result.Fail("Library can't be removed because it doesn't even exist — which, if you think about it, is kind of a win already.");
        else 
            return Result.Success("Operation allowed. Library exists and maybe can be removed. Let's see.");

    }

}
