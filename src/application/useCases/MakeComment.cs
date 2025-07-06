using Flexlib.Application.Ports;
using Flexlib.Application.UseCases.Common;
using Flexlib.Common;
using Flexlib.Domain;
using System.Text;

namespace Flexlib.Application.UseCases;

public static class MakeComment
{
    public static Result Execute(string itemName, string libName, string text, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(itemName, libName, text, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _MakeComment(parsedArgs)
            : validation;
    }

    private static Result _MakeComment(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;
        if (selectedLibrary == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' not found.");
        
        var selectedItem = selectedLibrary.GetItemByName(parsedArgs.ItemName);

        if (selectedItem == null)
            return Result.Fail($"Item '{parsedArgs.ItemName}' not found in library '{selectedLibrary.Name}'.");
        
        string id = $"{selectedItem.GetCommentCount() + 1}";

        var comment = new Comment(id, parsedArgs.Text);

        selectedItem.AddComment(comment);

        parsedArgs.Repo.Save(selectedLibrary);
         
        return Result.Success($"Comment of ID {id} was added to the item {selectedItem.Name} at library {selectedLibrary.Name}");

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
        public string Text { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(string itemName, string libName, string text, ILibraryRepository repo)
        {
            LibName = libName;
            ItemName = itemName;
            Text = text ?? "";
            Repo = repo;
        }
    }
}

