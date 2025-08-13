using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Interop;


namespace Flexlib.Application.Common;

public static class SelectItemNote
{
    public static Result Execute(object itemId, string noteId, string libName, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(itemId, noteId, libName, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _EditNote(parsedArgs)
            : validation;
    }

    private static Result _EditNote(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;
        
        var selectedItem = selectedLibrary.GetItemById(parsedArgs.ItemId);

        var selectedNote = selectedItem!.Notes.FirstOrDefault(c => c.Id.ToLowerInvariant() == parsedArgs.NoteId.ToLowerInvariant());

        return Result.Success("", selectedNote);
    }

    private static Result IsOperationAllowed(ParsedArgs parsedArgs)
    {

        if (string.IsNullOrWhiteSpace(parsedArgs.LibName))
            return Result.Fail("Library name must be informed.");

        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName);
        if (selectedLibrary == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' not found.");

        var selectedItem = selectedLibrary.GetItemById(parsedArgs.ItemId);
        
        if (selectedItem == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' has no item with ID '{parsedArgs.ItemId}'.");
        
        if (!selectedItem.Notes.Any(c => c.Id == parsedArgs.NoteId))
            return Result.Fail($"Note with id {parsedArgs.NoteId} not found.");

        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public object ItemId { get; }
        public string NoteId { get; }
        public string LibName { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(object itemId, string noteId, string libName, ILibraryRepository repo)
        {
            LibName = libName;
            NoteId = noteId;
            ItemId = itemId;
            Repo = repo;
        }
    }
}