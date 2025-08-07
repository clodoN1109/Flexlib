using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Interop;
using Flexlib.Domain;
using System.Text;

namespace Flexlib.Application.UseCases;

public static class NewNote
{
    public static Result Execute(object itemId, string libName, string text, IUser user, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(itemId, libName, text, user, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _NewNote(parsedArgs)
            : validation;
    }

    private static Result _NewNote(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;
        
        var selectedItem = selectedLibrary!.GetItemById(parsedArgs.ItemId);
       
        string id = $"{selectedItem!.GetNoteCount() + 1}";

        var note = new Note(id, parsedArgs.Text, parsedArgs.Author);

        selectedItem.NewNote(note);

        parsedArgs.Repo.Save(selectedLibrary);
         
        return Result.Success($"Note of ID {id} was added to the item {selectedItem.Name} at library {selectedLibrary.Name}");

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

