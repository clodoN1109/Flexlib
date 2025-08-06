using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Interop;
using Flexlib.Domain;
using System.Text;

namespace Flexlib.Application.UseCases;

public static class RenameItem
{
    public static Result Execute(object itemId, string newName, string libName, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(itemId, newName, libName, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _RenameItem(parsedArgs)
            : validation;
    }

    private static Result _RenameItem(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName);
        if (selectedLibrary is null)
            return Result.Fail($"Library '{parsedArgs.LibName}' not found.");

        var selectedItem = selectedLibrary.GetItemById(parsedArgs.ItemId);
        if (selectedItem is null)
            return Result.Fail($"Item '{parsedArgs.ItemId}' not found in library '{parsedArgs.LibName}'.");

        if (string.IsNullOrWhiteSpace(parsedArgs.NewName))
            return Result.Fail("New item name must be provided.");

        return parsedArgs.Repo.RenameItem(selectedItem, parsedArgs.NewName, selectedLibrary);

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
        
        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public object ItemId { get; }
        public string NewName { get; }
        public string LibName { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(object itemId, string newName, string libName, ILibraryRepository repo)
        {
            ItemId = itemId;
            LibName = libName;
            NewName = newName;
            Repo = repo;    
        }
    }
}

