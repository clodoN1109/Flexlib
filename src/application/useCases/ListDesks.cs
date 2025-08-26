using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Common;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases;

public static class ListDesks
{
    public static Result Execute(string libName, ILibraryRepository repo)
    {

        Result validation = IsOperationAllowed(libName, repo);

        if (validation.IsSuccess)
        {
            return _ListDesks(libName, repo);
        }
        else 
        {
            return validation;
        }

    }

    private static Result _ListDesks(string libName, ILibraryRepository repo)
    {
        try {
            var lib = repo.GetByName(libName);
            
            return Result.Success($"", lib?.Desks);
        }
        catch {
            return Result.Fail($"Failed to present list of desks from library {libName}");
        }
        
    }

    private static Result IsOperationAllowed(string libName, ILibraryRepository repo)
    {
        Library? selectedLibrary = repo.GetByName(libName);
        if (selectedLibrary == null)
            return Result.Fail($"Library '{libName}' not found.");

        return Result.Success("Operation allowed.");
    }
}

