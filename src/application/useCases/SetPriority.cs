using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Common;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases;


public static class SetPriority
{
    public static Result Execute(string newPriority, string itemId, string deskId, string libName, ILibraryRepository repo)
    {
        var validation = IsOperationAllowed(itemId, deskId, libName, repo);
        if (validation.IsFailure) return validation;

        var lib = repo.GetByName(libName)!;
        var item = lib.Desks.First(d => d.Id.ToString() == deskId).BorrowedItems.First(i => i.Id == itemId);

        int priorityLevel = 0;
        int.TryParse(newPriority, out priorityLevel);

        var result = item.SetPriority(priorityLevel);
        if (result.IsFailure)
            return result;

        return repo.Save(lib, true);
    }

    private static Result IsOperationAllowed(string itemId, string deskId, string libName, ILibraryRepository repo)
    {
        var lib = repo.GetByName(libName);
        if (lib == null) return Result.Fail($"Library '{libName}' not found.");

        var desk = lib.Desks.FirstOrDefault(d => d.Id.ToString() == deskId);
        if (desk == null) return Result.Fail($"Desk '{deskId}' not found.");

        var item = desk.BorrowedItems.FirstOrDefault(i => i.Id == itemId);
        if (item == null) return Result.Fail($"Item '{itemId}' not found.");

        return Result.Success("Operation allowed.");
    }
}

