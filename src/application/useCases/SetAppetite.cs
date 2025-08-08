using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Common;
using Flexlib.Application.Ports;
using System.Globalization;

namespace Flexlib.Application.UseCases;

public static class SetAppetite
{
    public static Result Execute(string itemId, string deskId, string libName, string date, ILibraryRepository repo)
    {
        Result validation = IsOperationAllowed(itemId, deskId, libName, repo);

        if (validation.IsSuccess)
            return _SetAppetite(itemId, deskId, libName, date, repo);

        return validation;
    }

    private static Result _SetAppetite(string itemId, string deskId, string libName, string date, ILibraryRepository repo)
    {
        var lib = repo.GetByName(libName)!;
        var desk = lib.Desks.First(d => d.Id.ToString() == deskId);
        var item = desk.BorrowedItems.First(i => i.Id == itemId);

        if (!DateTime.TryParseExact(date, "MM/dd/yy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var appetiteDate))
            return Result.Fail("Invalid date format. Please use MM/dd/yy HH:mm (e.g., 08/06/25 14:30)");

        var result = item.SetAppetite(appetiteDate);
        if (result.IsFailure)
            return result;

        return repo.Save(lib, true);
    }

    private static Result IsOperationAllowed(string itemId, string deskId, string libName, ILibraryRepository repo)
    {
        var lib = repo.GetByName(libName);
        if (lib == null)
            return Result.Fail($"Library '{libName}' not found.");

        var desk = lib.Desks.FirstOrDefault(d => d.Id.ToString() == deskId);
        if (desk == null)
            return Result.Fail($"Desk '{deskId}' not found in library '{libName}'.");

        var item = desk.BorrowedItems.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return Result.Fail($"Item '{itemId}' not found in desk '{deskId}'.");

        return Result.Success("Operation allowed.");
    }
}

