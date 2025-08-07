using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Common;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases;

public static class NewDesk
{
    public static Result Execute(string deskName, string libName, ILibraryRepository repo)
    {

        Result validation = IsOperationAllowed(deskName, libName, repo);

        if (validation.IsSuccess)
        {
            return _NewDesk(deskName, libName, repo);
        }
        else 
        {
            return validation;
        }

    }

    private static Result _NewDesk(string deskName, string libName, ILibraryRepository repo)
    {
        Result result;
        
        var lib = repo.GetByName(libName);
        result = lib!.NewDesk(deskName);
        if (result.IsFailure)
            return result;

        return repo.Save(lib);
        
    }

    private static Result IsOperationAllowed(string deskName, string libName, ILibraryRepository repo)
    {
        Library? selectedLibrary = repo.GetByName(libName);
        if (selectedLibrary == null)
            return Result.Fail($"Library '{libName}' not found.");

        return Result.Success("Operation allowed.");
    }
}

