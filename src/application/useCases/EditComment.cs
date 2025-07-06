using Flexlib.Application.Ports;
using Flexlib.Application.UseCases.Common;
using Flexlib.Common;
using Flexlib.Domain;
using System.Text;

namespace Flexlib.Application.UseCases;

public static class EditComment
{
    public static Result Execute(string itemName, string libName, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(itemName, libName, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _EditComment(parsedArgs)
            : validation;
    }

    private static Result _EditComment(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;
        if (selectedLibrary == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' not found.");
        
        var selectedItem = selectedLibrary.GetItemByName(parsedArgs.ItemName);

        if (selectedItem == null)
            return Result.Fail($"Item '{parsedArgs.ItemName}' not found in library '{selectedLibrary.Name}'.");

        return Result.Success("");
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

        if (!selectedLibrary.ContainsName(parsedArgs.ItemName))
            return Result.Fail($"Library '{parsedArgs.LibName}' has no item named '{parsedArgs.ItemName}'.");

        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public string ItemName { get; }
        public string LibName { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(string itemName, string libName, ILibraryRepository repo)
        {
            LibName = libName;
            ItemName = itemName;
            Repo = repo;
        }
    }
}

