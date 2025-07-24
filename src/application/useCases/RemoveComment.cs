using Flexlib.Application.Ports;
using Flexlib.Application.UseCases.Common;
using Flexlib.Common;
using Flexlib.Domain;
using System.Text;
using Flexlib.Interface;

namespace Flexlib.Application.UseCases;

public static class RemoveComment
{
    public static Result Execute(string itemName, string commentId, string libName, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(itemName, commentId, libName, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _RemoveComment(parsedArgs)
            : validation;
    }

    private static Result _RemoveComment(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;
        
        var selectedItem = selectedLibrary.GetItemByName(parsedArgs.ItemName);

        selectedItem!.RemoveComment(parsedArgs.CommentId);

        parsedArgs.Repo.Save(selectedLibrary);

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

        var selectedItem = selectedLibrary.GetItemByName(parsedArgs.ItemName);
        
        if (selectedItem == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' has no item named '{parsedArgs.ItemName}'.");
        
        if (!selectedItem.Comments.Any(c => c.Id == parsedArgs.CommentId))
            return Result.Fail($"Comment with id {parsedArgs.CommentId} not found.");

        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public string ItemName { get; }
        public string CommentId { get; }
        public string LibName { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(string itemName, string commentId, string libName, ILibraryRepository repo)
        {
            LibName = libName;
            CommentId = commentId;
            ItemName = itemName;
            Repo = repo;
        }
    }
}

