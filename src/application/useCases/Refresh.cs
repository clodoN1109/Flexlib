using Flexlib.Application.Ports;
using Flexlib.Domain;
using Flexlib.Common;

namespace Flexlib.Application.UseCases;

public static class Refresh
{
    public static Result Execute(string? libName, ILibraryRepository repo)
    {

        Result validation = IsOperationAllowed(libName, repo);

        if (validation.IsSuccess)
        {
            if (libName != null)
            {
                return RefreshLibrary(libName, repo);
            }
            else
            {
                RefreshAllLibraries(repo);
                return Result.Success("All Libraries refreshed.");
            }
        }
        else 
        {
            return validation;
        }

    }

    private static Result RefreshLibrary(string libName, ILibraryRepository repo)
    {
        Library? lib = repo.GetByName(libName);

        if (lib != null)
        {
            repo.Save(lib);
            return Result.Success("Library refreshed.");
        }
        else
        {
            return Result.Fail($"Library {libName} not found.");
        }
    }
    
    private static void RefreshAllLibraries(ILibraryRepository repo)
    {
        foreach (Library lib in repo.GetAll().ToList())
        {
            repo.Save(lib);
        }
        
    }
    
    private static Result IsOperationAllowed(string? libName, ILibraryRepository repo)
    {

        if (libName == null)
            return Result.Success("Operation allowed.");

        Library? selectedLibrary = repo.GetByName(libName);
        if (selectedLibrary == null)
            return Result.Fail($"Library '{libName}' not found.");

        return Result.Success("Operation allowed.");
    }
}

