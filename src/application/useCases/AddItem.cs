using Flexlib.Domain;
using Flexlib.Common;
using Flexlib.Application.UseCases.Common;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases;

public static class AddItem
{
    public static Result Execute(string libName, string itemOrigin, string itemName, ILibraryRepository repo)
    {

        Result validation = IsOperationAllowed(libName, itemOrigin, itemName, repo);

        if (validation.IsSuccess)
        {
            return AddItemToLib(libName, itemOrigin, itemName, repo);
        }
        else 
        {
            return validation;
        }

    }

    private static Result AddItemToLib(string libName, string itemOrigin, string itemName, ILibraryRepository repo)
    {
        Library? lib = repo.GetByName(libName);
        if (lib != null)
        {
            repo.Save(lib.AddItem(itemName, itemOrigin)); 
            return Result.Success($"Item '{itemName}' added to the library '{libName}'.");
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

