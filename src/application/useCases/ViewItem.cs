using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Interop;
using Flexlib.Domain;
using System.Text;

namespace Flexlib.Application.UseCases;

public static class GetItemLocalCopy
{
    public static Result Execute(object itemId, string libName, string application, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(itemId, libName, application, repo); 

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

        return Result.Success("Successfully retrieved the item's local copy.", localCopy);

    }

    private static Result IsOperationAllowed(ParsedArgs parsedArgs)
    {
        

        
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
        public ParsedArgs(object itemId, string libName, string application, ILibraryRepository repo)
        {
            ItemId = itemId;
            LibName = libName;
            Application = application;
            Repo = repo; 
        }
    }
}

