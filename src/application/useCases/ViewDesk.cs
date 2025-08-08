using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Common;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases;

public static class ViewDesk
{
    public static Result Execute(string deskId, string libName, string sortSequence, ILibraryRepository repo, IPresenter presenter)
    {

        Result validation = IsOperationAllowed(deskId, libName, repo);

        if (validation.IsSuccess)
        {
            return _ViewDesk(deskId, libName, sortSequence, repo, presenter);
        }
        else 
        {
            return validation;
        }

    }

    private static Result _ViewDesk(string deskId, string libName, string sortSequence, ILibraryRepository repo, IPresenter presenter)
    {
        
        try {
            var lib = repo.GetByName(libName);
            var desk = lib!.GetDeskById(deskId);

            desk!.Sort( new SortSequence(sortSequence).Elements );

            presenter.ViewDesk(desk!, libName);
            
            return Result.Success($"");
        }
        catch {
            return Result.Fail($"");
        }
        
    }

    private static Result IsOperationAllowed(string deskId, string libName, ILibraryRepository repo)
    {
        Library? lib = repo.GetByName(libName);
        if (lib == null)
            return Result.Fail($"Library '{libName}' not found.");

        var desk = lib.GetDeskById(deskId); 
        if (desk == null)
            return Result.Fail($"Desk of ID '{deskId}' not found.");
        
        return Result.Success("Operation allowed.");
    }
}

