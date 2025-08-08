using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Common;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases;


public static class DefineProgress
{
    public static Result Execute(string unit, string completionValue, string itemId, string deskId, string libName, ILibraryRepository repo)
    {
        var validation = IsOperationAllowed(itemId, deskId, libName, repo);
        if (validation.IsFailure) return validation;

        var lib = repo.GetByName(libName)!;
        var item = lib.Desks.First(d => d.Id.ToString() == deskId).BorrowedItems.First(i => i.Id == itemId);

        item.Progress.Unit = unit;
        item.Progress.CompletionValue = completionValue;

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

