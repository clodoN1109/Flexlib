using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Common;
using Flexlib.Domain;
using System.Text;
using Flexlib.Interface;

namespace Flexlib.Application.UseCases;

public static class EditComment
{
    public static Result Execute(object itemId, string commentId, string libName, IReader reader, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(itemId, commentId, libName, reader, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _EditComment(parsedArgs)
            : validation;
    }

    private static Result _EditComment(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;
        
        var selectedItem = selectedLibrary.GetItemById(parsedArgs.ItemId);

        var selectedComment = selectedItem!.Comments.FirstOrDefault(c => c.Id == parsedArgs.CommentId);

        var currentText = selectedComment!.Text; 

        selectedComment.Text = (parsedArgs.Reader.ReadText(currentText) ?? "").Trim();

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
        public IReader Reader { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(object itemId, string commentId, string libName, IReader reader, ILibraryRepository repo)
        {
            LibName = libName;
            CommentId = commentId;
            ItemId = itemId;
            Reader = reader;
            Repo = repo;
        }
    }
}

