using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Ports;
using Flexlib.Application.UseCases;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Persistence;
using Flexlib.Infrastructure.Authorization;
using Flexlib.Interface.Input;
using Flexlib.Interface.Output;
using Flexlib.Domain;
using Flexlib.Interface.Input.Commands;

namespace Flexlib.Interface.Controllers;


public static class CLIController
{

    private static readonly ILibraryRepository _libRepo = new JsonLibraryRepository();
    private static readonly IPresenter _presenter = new ConsolePresenter();
    private static readonly IReader _reader = new Reader();

    private static LibraryItem? _selectedItem { get; set; }
    private static Library? _selectedLibrary { get; set; }
    private static string? _input { get; set; } 
    private static object? _payload { get; set; }
    private static Result? _result { get; set; }
    public static Result Handle(Command cmd, IUser authUser)
    {
        if (Authorization.IsNotAuthorized(cmd, authUser))
        {
            _result = Result.Fail($"User {authUser.Name} is not authorized to perform action {cmd.Type}.");
            _presenter.Result(_result);
            return _result;
        }

        _result = Execute(cmd, authUser);
        _presenter.Result(_result);

        return _result;

    }

    private static Result Execute(Command cmd, IUser authUser)
    {

        switch (cmd)
        {

            // Configuration
            case SelectProfileCommand c:
                return SelectProfile.Execute(c.Name, _libRepo);

            // Libraries
            case NewLibraryCommand c:
                return NewLibrary.Execute(c.Name, c.Path, _libRepo);

            case ListLibrariesCommand c:
                _result = ListLibs.Execute(_libRepo);
                if (_result.Payload is List<Library> libs)
                {
                    _presenter.ListLibs(libs);
                }
                return _result;
                
            case GetLibraryLayoutCommand c:
                _result = GetLibraryLayout.Execute(c.LibraryName, _libRepo);

                if (_result.Payload is not List<string> layout)
                    return _result;
                    
                if (layout.Count > 0)
                {
                    _presenter.PresentLayoutSequence(layout);
                    return Result.Success("");
                }
                else
                {
                    return Result.Fail("Layout sequence is empty.");
                }

            case SetLibraryLayoutCommand c:
                return SetLibraryLayout.Execute(c.LibraryName, c.LayoutString, _libRepo);

            case RemoveLibraryCommand c:

                _selectedLibrary = _libRepo.GetByName(c.Name)!;
                Console.WriteLine($"\nAre you sure you want to delete the library '{c.Name}' at path:\n\n  {_selectedLibrary.Path} ?\n");
                Console.Write("(y/N) > ");
                _input = Console.ReadLine();

                if (!string.Equals(_input, "y", StringComparison.OrdinalIgnoreCase))
                {
                    return Result.Fail("Deletion cancelled by user.");
                }
                return RemoveLibrary.Execute(c.Name, _libRepo);

            // Items
            case NewItemCommand c:
                return NewItem.Execute(c.LibraryName, c.ItemOrigin, c.ItemName, _libRepo);

            case RemoveItemCommand c:
                _selectedLibrary = _libRepo.GetByName(c.LibraryName)!;
                _selectedItem = _selectedLibrary.GetItemById(c.ItemId);
                Console.WriteLine($"\nAre you sure you want to delete the item '{_selectedItem?.Name ?? ""}' from library '{_selectedLibrary?.Name ?? ""}'?\n\n");
                Console.Write("(y/N) > ");
                _input = Console.ReadLine();
                if (!string.Equals(_input, "y", StringComparison.OrdinalIgnoreCase))
                    return Result.Fail("Deletion cancelled.");
                return RemoveItem.Execute(c.ItemId, c.LibraryName, _libRepo);

            case ListItemsCommand c:
                _result = ListItems.Execute(c.LibraryName, c.FilterSequence, c.SortSequence, c.ItemName, _libRepo);
                if (_result.Payload is ListItemsPayload payload)
                    _presenter.ListItems(payload.Items,
                                    payload.Library,
                                    payload.FilterSequence,
                                    payload.SortSequence,
                                    payload.LocalSizeInBytes,
                                    payload.ItemNameFilter
                                    );
                return _result;

            case RenameItemCommand c:
                return RenameItem.Execute(c.ItemId, c.NewName, c.LibraryName, _libRepo);

            case UpdateItemOriginCommand c:
                return UpdateItemOrigin.Execute(c.ItemId, c.NewOrigin, c.LibraryName, _libRepo);

            case GetItemOriginCommand c:
                _payload = GetItemOrigin.Execute(c.ItemId, c.LibraryName, _libRepo).Payload;
                if (_payload is not string currentOrigin)
                    return Result.Fail($"Could not retrive the origin of the item with ID {c.ItemId}.");

                if (currentOrigin.Length == 0)
                    return Result.Fail($"The item has no defined origin.");

                return Result.Success($"The current origin for item of ID {c.ItemId} is {currentOrigin}.");

            case ViewItemCommand c:
                _result = GetItemLocalCopy.Execute(c.ItemId, c.LibraryName, c.Application, _libRepo);
                if (_result.Payload is string localCopy)
                    _result =_presenter.File(localCopy);
                return _result;

            // Desks
            case NewDeskCommand c:
                return NewDesk.Execute(c.DeskName, c.LibraryName, _libRepo);

            case ListDesksCommand c:
                _result =  ListDesks.Execute(c.LibraryName, _libRepo);
                if (_result.Payload is List<Desk> desks)
                    _presenter.ListDesks(desks, c.LibraryName);
                return _result;
            case ViewDeskCommand c:
                _result =  ViewDesk.Execute(c.DeskId, c.LibraryName, c.SortSequence, _libRepo);
                if (_result.Payload is Desk desk)
                    _presenter.ViewDesk(desk, c.LibraryName);
                return _result;

            case SetAppetiteCommand c:
                return SetAppetite.Execute(c.ItemID, c.DeskID, c.LibraryName, c.Date, _libRepo);

            case SetProgressCommand c:
                return SetProgress.Execute(c.NewValue, c.ItemID, c.DeskID, c.LibraryName, _libRepo);

            case DefineProgressCommand c:
                return DefineProgress.Execute(c.Unit, c.CompletionValue, c.ItemID, c.DeskID, c.LibraryName, _libRepo);

            case SetPriorityCommand c:
                return SetPriority.Execute(c.NewPriority, c.ItemID, c.DeskID, c.LibraryName, _libRepo);

            case RenameDeskCommand c:
                return RenameDesk.Execute(c.NewName, c.DeskID, c.LibraryName, _libRepo);

            case BorrowItemCommand c:
                return BorrowItem.Execute(c.ItemId, c.DeskId, c.LibraryName, authUser.Id, _libRepo);

            case ListLoansCommand c:
                _result = ListLoans.Execute(c.ItemId, c.LibraryName, _libRepo);
                if (_result.Payload is (LoanHistory loans, LibraryItem libItem))
                    _presenter.PresentLoanHistory(loans, libItem, c.LibraryName);
                return _result;

            case ReturnItemCommand c:
                return ReturnItem.Execute(c.ItemId, c.DeskId, c.LibraryName, authUser.Id, _libRepo);

            // Local Storage
            case FetchFilesCommand c:
                return FetchFiles.Execute(c.LibraryName, _libRepo);

            case RebalanceLocalStorageCommand c:
                return RebalanceLocalStorage.Execute(c.LibraryName, _libRepo);

            // Notes
            case NewNoteCommand c:
                var note = string.IsNullOrWhiteSpace(c.Note)
                    ? _reader.ReadText()
                    : c.Note;

                if (string.IsNullOrWhiteSpace(note))
                    return Result.Fail("Failed to get text input.");

                return NewNote.Execute(c.ItemId, c.LibName, note, authUser, _libRepo);

            case ListNotesCommand c: 
                _result = ListNotes.Execute(c.ItemId, c.LibName, _libRepo);
                if (_result.Payload is (List<Note> notes, LibraryItem item))
                    _presenter.ListNotes(notes, item.Name ?? "", item.Id, c.LibName ?? "");
                return _result;
                
            case EditNoteCommand c:
                string? newNote = "";
                _payload = SelectItemNote.Execute(c.ItemId, c.NoteId, c.LibName, _libRepo).Payload;

                if ((_payload is Domain.Note currentNote) && !string.IsNullOrWhiteSpace(currentNote.Text))
                    newNote = _reader.ReadText(currentNote.Text);

                if (string.IsNullOrWhiteSpace(newNote))
                    return Result.Fail("Failed to get any text input.");

                return EditNote.Execute(c.ItemId, c.NoteId, c.LibName, newNote, _libRepo);

            case RemoveNoteCommand c:
                return RemoveNote.Execute(c.ItemId, c.NoteId, c.LibName, _libRepo);

            // Properties
            case NewPropertyCommand c:
                return NewProperty.Execute(c.LibName, c.PropName, c.PropType, _libRepo);

            case ListPropertiesCommand c:
                _result = ListProperties.Execute(c.LibName, c.ItemId, _libRepo);
                if (_result.Payload is Library lib)
                    _presenter.LibraryProperties(lib);
                else if (_result.Payload is (Library library, LibraryItem i))
                    _presenter.ItemProperties(i, library);                
                return _result;

            case SetPropertyCommand c:
                return SetProperty.Execute(c.PropName, c.NewValue, c.LibName, c.ItemId, _libRepo);

            case RenamePropertyCommand c:
                return RenameProperty.Execute(c.PropName, c.NewName, c.LibName, _libRepo);

            case UnsetPropertyCommand c:
                return UnsetProperty.Execute(c.PropName, c.TargetValue, c.LibName, c.ItemId, _libRepo);

            case RemovePropertyCommand c:
                return RemoveProperty.Execute(c.PropName, c.LibName, _libRepo);
            // Unknown Command   
            default:
                return Result.Fail($"Unknown use case: {cmd?.GetType().Name ?? "null"}");
        }
         
    }
}

