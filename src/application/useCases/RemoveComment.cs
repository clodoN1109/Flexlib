using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Interop;
using Flexlib.Domain;
using System.Text;
using Flexlib.Interface;

namespace Flexlib.Application.UseCases;

public static class RemoveComment
{
    public static Result Execute(object itemId, string commentId, string libName, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(itemId, commentId, libName, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _RemoveComment(parsedArgs)
            : validation;
    }

    private static Result _RemoveComment(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;
        
        var selectedItem = selectedLibrary.GetItemById(parsedArgs.ItemId);

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

        var selectedItem = selectedLibrary.GetItemById(parsedArgs.ItemId);
        
        if (selectedItem == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' has no item with ID '{parsedArgs.ItemId}'.");
        
        if (!selectedItem.Comments.Any(c => c.Id == parsedArgs.CommentId))
            return Result.Fail($"Comment with id {parsedArgs.CommentId} not found.");

        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public object ItemId { get; }
        public string CommentId { get; }
        public string LibName { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(object itemId, string commentId, string libName, ILibraryRepository repo)
        {
            LibName = libName;
            CommentId = commentId;
            ItemId = itemId;
            Repo = repo;
        }
    }
}

