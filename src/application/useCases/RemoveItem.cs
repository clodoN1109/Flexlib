using Flexlib.Application.Ports;
using Flexlib.Application.UseCases.Common;
using Flexlib.Common;
using Flexlib.Domain;
using System.Text;

namespace Flexlib.Application.UseCases;

public static class RemoveItem
{
    public static Result Execute(object itemId, string libName, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(itemId, libName, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _RemoveItem(parsedArgs)
            : validation;
    }

    private static Result _RemoveItem(ParsedArgs parsedArgs)
    {

        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;
        
        var selectedItem = selectedLibrary.GetItemById(parsedArgs.ItemId);
        if (selectedItem == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' has no item with ID '{parsedArgs.ItemId}'.");
        
        return parsedArgs.Repo.RemoveItem(selectedItem, selectedLibrary);

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
        public string LibName { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(object itemId, string libName, ILibraryRepository repo)
        {
            ItemId = itemId;
            LibName = libName;
            Repo = repo;    
        }
    }
}

