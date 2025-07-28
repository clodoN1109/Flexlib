using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Common;
using Flexlib.Domain;
using System.Text;

namespace Flexlib.Application.UseCases;

public static class NewComment
{
    public static Result Execute(object itemId, string libName, string text, IUser user, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(itemId, libName, text, user, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _NewComment(parsedArgs)
            : validation;
    }

    private static Result _NewComment(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;
        
        var selectedItem = selectedLibrary!.GetItemById(parsedArgs.ItemId);
       
        string id = $"{selectedItem!.GetCommentCount() + 1}";

        var comment = new Comment(id, parsedArgs.Text, parsedArgs.Author);

        selectedItem.NewComment(comment);

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

        if (!selectedLibrary.ContainsId(parsedArgs.ItemId))
            return Result.Fail($"Library '{parsedArgs.LibName}' has no item with ID '{parsedArgs.ItemId}'.");

        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public object ItemId { get; }
        public string LibName { get; }
        public string Text { get; }
        public IUser Author { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(object itemId, string libName, string text, IUser author, ILibraryRepository repo)
        {
            LibName = libName;
            ItemId = itemId;
            Text = text ?? "";
            Author = author;
            Repo = repo;
        }
    }
}

