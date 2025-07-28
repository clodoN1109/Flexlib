using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Common;
using Flexlib.Domain;
using System.Text;


namespace Flexlib.Application.UseCases;

public static class ListComments
{
    public static Result Execute(object itemId, string libName, ILibraryRepository repo, IPresenter presenter)
    {
        var parsedArgs = new ParsedArgs(itemId, libName, repo, presenter); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _ListComments(parsedArgs)
            : validation;
    }

    private static Result _ListComments(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;
        
        var selectedItem = selectedLibrary.GetItemById(parsedArgs.ItemId);

        parsedArgs.Presenter.ListComments(selectedItem!.Comments, selectedItem!.Name ?? "", selectedLibrary!.Name ?? "");

        return Result.Success($"");

    }

    private static Result IsOperationAllowed(ParsedArgs parsedArgs)
    {
        if (parsedArgs.LibName == "Default Library" && AssureDefaultLibrary.Execute(parsedArgs.Repo).IsFailure)
            return Result.Fail($"Default Library not found.");
        
        if (string.IsNullOrWhiteSpace(parsedArgs.LibName))
            return Result.Fail("Library name must be informed.");

        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName);
        if (selectedLibrary == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' not found.");

        if (!selectedLibrary.ContainsId(parsedArgs.ItemId))
            return Result.Fail($"Library '{parsedArgs.LibName}' has no item with ID '{parsedArgs.ItemId}'.");

        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public object ItemId { get; }
        public string LibName { get; }
        public ILibraryRepository Repo { get; }
        public IPresenter Presenter { get; }

        public ParsedArgs(object itemId, string libName, ILibraryRepository repo, IPresenter presenter)
        {
            LibName = libName;
            ItemId = itemId;
            Repo = repo;
            Presenter = presenter;
        }
    }
}

