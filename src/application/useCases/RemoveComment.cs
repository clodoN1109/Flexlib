using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Interop;
using Flexlib.Domain;
using System.Text;
using Flexlib.Interface;

namespace Flexlib.Application.UseCases;

public static class RemoveNote
{
    public static Result Execute(object itemId, string noteId, string libName, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(itemId, noteId, libName, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _RemoveNote(parsedArgs)
            : validation;
    }

    private static Result _RemoveNote(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;
        
        var selectedItem = selectedLibrary.GetItemById(parsedArgs.ItemId);

        selectedItem!.RemoveNote(parsedArgs.NoteId);

        parsedArgs.Repo.Save(selectedLibrary);

        return Result.Success("");
    }

    private static Result IsOperationAllowed(ParsedArgs parsedArgs)
    {

        if (parsedArgs.LibName.ToLowerInvariant() == "Default Library".ToLowerInvariant() && AssureDefaultLibrary.Execute(parsedArgs.Repo).IsFailure)
            return Result.Fail($"Default Library not found.");
        
        if (string.IsNullOrWhiteSpace(parsedArgs.LibName))
            return Result.Fail("Library name must be informed.");

        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName);
        if (selectedLibrary == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' not found.");

        var selectedItem = selectedLibrary.GetItemById(parsedArgs.ItemId);
        
        if (selectedItem == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' has no item with ID '{parsedArgs.ItemId}'.");
        
        if (!selectedItem.Notes.Any(c => c.Id.ToLowerInvariant() == parsedArgs.NoteId.ToLowerInvariant()))
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

