using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Common;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases;

public static class NewItem
{
    public static Result Execute(string libName, string itemOrigin, string itemName, ILibraryRepository repo)
    {

        Result validation = IsOperationAllowed(libName, itemOrigin, itemName, repo);

        if (validation.IsSuccess)
        {
            return NewItemToLib(libName, itemOrigin, itemName, repo);
        }
        else 
        {
            return validation;
        }

    }

    private static Result NewItemToLib(string libName, string itemOrigin, string itemName, ILibraryRepository repo)
    {
        Library? lib = repo.GetByName(libName);
        if (lib != null)
        {
            var newItem = lib.NewItem(itemName, itemOrigin);
            var result = repo.Save(newItem, lib); 
            return result;
        }
        else {
            return Result.Fail($"Library '{libName}' not found.");
        }
        
    }

    private static Result IsOperationAllowed(string libName, string itemOrigin, string itemName, ILibraryRepository repo)
    {
        if (libName == "Default Library" && AssureDefaultLibrary.Execute(repo).IsFailure)
            return Result.Fail($"Default Library not found.");

        Library? selectedLibrary = repo.GetByName(libName);
        if (selectedLibrary == null)
            return Result.Fail($"Library '{libName}' not found.");

        return Result.Success("Operation allowed.");
    }
}

