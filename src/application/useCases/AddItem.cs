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

        LibraryItem? sameNameItem = selectedLibrary.GetItemByName(itemName);
        LibraryItem? sameOriginItem = selectedLibrary.GetItemByOrigin(itemOrigin);

        if (sameNameItem != null)
            return Result.Fail($"An item named '{sameNameItem.Name}' already exists in library '{libName}' with origin '{sameNameItem!.Origin}'.");

        if (sameOriginItem != null)
            return Result.Fail($"An item with the same origin '{sameOriginItem!.Origin}' already exists in library '{libName}' as '{sameOriginItem!.Name}'.");

        return Result.Success("Operation allowed.");
    }
}

