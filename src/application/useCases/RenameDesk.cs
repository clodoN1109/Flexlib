using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Common;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases;


public static class RenameDesk
{
    public static Result Execute(string newName, string deskId, string libName, ILibraryRepository repo)
    {
        var validation = IsOperationAllowed(deskId, libName, repo);
        if (validation.IsFailure) return validation;

        var lib = repo.GetByName(libName)!;
        var desk = lib.Desks.First(d => d.Id.ToString() == deskId);

        desk.Name = newName;

        return repo.Save(lib, true);
    }

    private static Result IsOperationAllowed(string deskId, string libName, ILibraryRepository repo)
    {
        var lib = repo.GetByName(libName);
        if (lib == null) return Result.Fail($"Library '{libName}' not found.");

        var desk = lib.Desks.FirstOrDefault(d => d.Id.ToString() == deskId);
        if (desk == null) return Result.Fail($"Desk '{deskId}' not found.");

        return Result.Success("Operation allowed.");
    }
}

