using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Interop;
using Flexlib.Domain;
using System.Text;

namespace Flexlib.Application.UseCases;

public static class ListProperties
{
    public static Result Execute(string libName, string itemId, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(libName, itemId, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? ListProps(parsedArgs)
            : validation;
    }

    private static Result ListProps(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;

        if (string.IsNullOrWhiteSpace(parsedArgs.ItemId))
        {
            return ListPropertyDefinitions(selectedLibrary, parsedArgs);
        }

        return ListItemPropertiesWithValues(selectedLibrary, parsedArgs);
    }

    private static Result ListPropertyDefinitions(Library selectedLibrary, ParsedArgs parsedArgs)
    {
        if (selectedLibrary == null)
            return Result.Fail($"Could not retrieve library named {parsedArgs.LibName}.");
        return Result.Success("", selectedLibrary);
    }

    private static Result ListItemPropertiesWithValues(Library selectedLibrary, ParsedArgs parsedArgs)
    {

        var selectedItem = selectedLibrary.GetItemById(parsedArgs.ItemId);
        if (selectedItem == null)
            return Result.Fail($"Could not retrieve item of ID {parsedArgs.ItemId}.");
        return Result.Success("", (selectedLibrary, selectedItem));
    }

    private static Result IsOperationAllowed(ParsedArgs parsedArgs)
    {

        
        if (string.IsNullOrWhiteSpace(parsedArgs.LibName))
            return Result.Fail("Library name must be informed.");

        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName);
        if (selectedLibrary == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' not found.");

        if (string.IsNullOrWhiteSpace(parsedArgs.ItemId))
            return Result.Success("Operation allowed.");

        if (!selectedLibrary.PropertyDefinitions.Any())
            return Result.Success("This library has no property definitions.");
        
        if (parsedArgs.ItemId != null)
        {
            var selectedItem = selectedLibrary.GetItemById(parsedArgs.ItemId);
            if (selectedItem == null)
                return Result.Fail($"Item with ID {parsedArgs.ItemId} not found.");
        }

        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public string LibName { get; }
        public string ItemId { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(string libName, string? itemId, ILibraryRepository repo)
        {
            LibName = libName ?? "";
            ItemId = itemId ?? "";
            Repo = repo;
        }
    }
}

