using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Common;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases;

public static class ListLoans
{
    public static Result Execute(string itemId, string libName, ILibraryRepository repo)
    {

        Result validation = IsOperationAllowed(itemId, libName, repo);

        if (validation.IsSuccess)
        {
            return _ListLoans(itemId, libName, repo);
        }
        else 
        {
            return validation;
        }

    }

    private static Result _ListLoans(string itemId, string libName, ILibraryRepository repo)
    {
        try
        {
            var lib = repo.GetByName(libName);
            if (lib is null)
                return Result.Fail($"Library '{libName}' not found.");

            var item = lib.GetItemById(itemId);
            if (item is null)
                return Result.Fail($"Item with ID '{itemId}' not found in library '{libName}'.");

            return Result.Success($"Obtained loan history for item '{item.Name}' in library '{libName}'.", (item.Loans, item));
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while listing loan history: {ex.Message}");
        }
    }

    private static Result IsOperationAllowed(string itemId, string libName, ILibraryRepository repo)
    {
        Library? lib = repo.GetByName(libName);
        if (lib == null)
            return Result.Fail($"Library '{libName}' not found.");

        var item = lib.GetItemById(itemId); 
        if (item == null)
            return Result.Fail($"Item of ID '{itemId}' not found.");
        
        return Result.Success("Operation allowed.");
    }
}

