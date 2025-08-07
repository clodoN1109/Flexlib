using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Common;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases;

public static class BorrowItem
{
    public static Result Execute(string itemId, string deskId, string libName, string userId, ILibraryRepository repo)
    {

        Result validation = IsOperationAllowed(itemId, deskId, libName, repo);

        if (validation.IsSuccess)
        {
            return _BorrowItem(itemId, deskId, libName, userId, repo);
        }
        else 
        {
            return validation;
        }

    }

    private static Result _BorrowItem(string itemId, string deskId, string libName, string userId, ILibraryRepository repo)
    {
        
        try {
            var lib = repo.GetByName(libName);

            lib!.BorrowItem(itemId, userId, deskId);

            repo.Save(lib);
            
            return Result.Success($"");
        }
        catch {
            return Result.Fail($"");
        }
        
    }

    private static Result IsOperationAllowed(string itemId, string deskId, string libName, ILibraryRepository repo)
    {
        Library? lib = repo.GetByName(libName);
        if (lib == null)
            return Result.Fail($"Library '{libName}' not found.");

        var desk = lib.GetDeskById(deskId); 
        if (desk == null)
            return Result.Fail($"Desk of ID '{deskId}' not found.");
        
        var item = lib.GetItemById(itemId); 
        if (item == null)
            return Result.Fail($"Item of ID '{itemId}' not found.");
        
        return Result.Success("Operation allowed.");
    }
}

