using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Interop;
using Flexlib.Domain;
using System.Text;

namespace Flexlib.Application.UseCases;

public static class ListProperties
{
    public static Result Execute(string libName, string itemName, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(libName, itemName, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? ListProps(parsedArgs)
            : validation;
    }

    private static Result ListProps(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;

        if (string.IsNullOrWhiteSpace(parsedArgs.ItemName))
        {
            return ListPropertyDefinitions(selectedLibrary);
        }

        return ListItemPropertiesWithValues(selectedLibrary, parsedArgs);
    }

    private static Result ListPropertyDefinitions(Library selectedLibrary)
    {
        if (!selectedLibrary.PropertyDefinitions.Any())
            return Result.Success("This library has no property definitions.");

        var sb = new StringBuilder();
        sb.AppendLine($"Property definitions in library '{selectedLibrary.Name}':\n");

        foreach (var def in selectedLibrary.PropertyDefinitions)
            sb.AppendLine($"- {def.Name} : {def.TypeName}");

        return Result.Success(sb.ToString());
    }

    private static Result ListItemPropertiesWithValues(Library selectedLibrary, ParsedArgs parsedArgs)
    {

        var selectedItem = selectedLibrary.GetItemByName(parsedArgs.ItemName);

        if (selectedItem == null)
            return Result.Fail($"Item '{parsedArgs.ItemName}' not found in library '{selectedLibrary.Name}'.");

        if (!selectedItem.PropertyValues.Any())
            return Result.Success($"Item '{selectedItem.Name}' has no property values.");

        var sb2 = new StringBuilder();
        sb2.AppendLine($"Property values for item '{selectedItem.Name}':\n");

        foreach (var kv in selectedItem.PropertyValues)
        {
            var value = kv.Value ?? "(null)";
            sb2.AppendLine($"- {kv.Key} = {value}");
        }

        return Result.Success(sb2.ToString());
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

        if (string.IsNullOrWhiteSpace(parsedArgs.ItemName))
            return Result.Success("Operation allowed.");

        if (!selectedLibrary.ContainsName(parsedArgs.ItemName))
            return Result.Fail($"Library '{parsedArgs.LibName}' has no item named '{parsedArgs.ItemName}'.");

        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public string LibName { get; }
        public string ItemName { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(string libName, string? itemName, ILibraryRepository repo)
        {
            LibName = libName ?? "";
            ItemName = itemName ?? "";
            Repo = repo;
        }
    }
}

