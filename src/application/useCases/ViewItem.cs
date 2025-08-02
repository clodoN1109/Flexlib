using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Interop;
using Flexlib.Domain;
using System.Text;

namespace Flexlib.Application.UseCases;

public static class ViewItem
{
    public static Result Execute(object itemId, string libName, string application, ILibraryRepository repo, IPresenter presenter)
    {
        var parsedArgs = new ParsedArgs(itemId, libName, application, repo, presenter); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _ViewItem(parsedArgs)
            : validation;
    }

    private static Result _ViewItem(ParsedArgs parsedArgs)
    {

        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;
        
        var selectedItem = selectedLibrary.GetItemById(parsedArgs.ItemId);
        
        var localCopy = parsedArgs.Repo.GetItemLocalCopy(selectedItem!, selectedLibrary!);
        
        return parsedArgs.Presenter.File(localCopy!);

    }

    private static Result IsOperationAllowed(ParsedArgs parsedArgs)
    {
        
        if (parsedArgs.LibName.ToLowerInvariant() == "Default Library".ToLowerInvariant() && AssureDefaultLibrary.Execute(parsedArgs.Repo).IsFailure)
            return Result.Fail($"Default Library not found.");
        
        if (string.IsNullOrWhiteSpace(parsedArgs.LibName))
            return Result.Fail("Library name must be informed.");

        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName);
        if (selectedLibrary == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' not found.");

        if (parsedArgs.ItemId == null)
            return Result.Fail($"Item ID must be informed.");

        var selectedItem = selectedLibrary.GetItemById(parsedArgs.ItemId);
        if (selectedItem == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' has no item with ID '{parsedArgs.ItemId}'.");
        
        var localCopy = parsedArgs.Repo.GetItemLocalCopy(selectedItem!, selectedLibrary!);
        if (localCopy == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' has no local copy of the item with ID '{parsedArgs.ItemId}'.");

        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public object ItemId { get; }
        public string LibName { get; }
        public string Application { get; }
        public ILibraryRepository Repo { get; }
        public IPresenter Presenter { get; }

        public ParsedArgs(object itemId, string libName, string application, ILibraryRepository repo, IPresenter presenter)
        {
            ItemId = itemId;
            LibName = libName;
            Application = application;
            Repo = repo; 
            Presenter = presenter; 
        }
    }
}

