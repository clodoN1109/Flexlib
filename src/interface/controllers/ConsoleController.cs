using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Ports;
using Flexlib.Application.UseCases;
using Flexlib.Infrastructure.Persistence;
using Flexlib.Infrastructure.Authentication;
using Flexlib.Infrastructure.Authorization;
using Flexlib.Interface.Input;
using Flexlib.Interface.CLI;
using Flexlib.Interface.Output;

namespace Flexlib.Interface.Controllers;


public static class ConsoleController
{

    private static readonly ILibraryRepository _libRepo = new JsonLibraryRepository();
    private static readonly IPresenter _presenter = new ConsolePresenter();
    private static readonly IReader _reader = new Reader();


    public static void Handle(Command cmd, IUser authUser)
    {
        if (Authorization.IsNotAuthorized(cmd, authUser))
        {
            _presenter.Result(Result.Fail($"User {authUser.Name} is not authorized to perform action {cmd.Type}.") );
            return;
        }

        var result = Execute(cmd, authUser);
        
        _presenter.AvailableActions( Authorization.GetAllAuthorizedActions(authUser) );
        _presenter.Result(result);
                 
    }

    private static Result Execute(Command cmd, IUser authUser)
    {

        switch (cmd)
        {
            case NewLibraryCommand c:
                return NewLibrary.Execute(c.Name, c.Path, _libRepo);

            case NewItemCommand c:
                return NewItem.Execute(c.LibraryName, c.ItemOrigin, c.ItemName, _libRepo);

            case RemoveItemCommand c:
                return RemoveItem.Execute(c.ItemId, c.LibraryName, _libRepo);
            
            case RenameItemCommand c:
                return RenameItem.Execute(c.ItemId, c.NewName, c.LibraryName, _libRepo);
            
            case UpdateItemOriginCommand c:
                return UpdateItemOrigin.Execute(c.ItemId, c.NewOrigin, c.LibraryName, _libRepo);
            
            case ViewItemCommand c:
                return ViewItem.Execute(c.ItemId, c.LibraryName, c.Application, _libRepo, _presenter);

            case NewDeskCommand c:
                return NewDesk.Execute(c.DeskName, c.LibraryName, _libRepo);

            case ListDesksCommand c:
                return ListDesks.Execute(c.LibraryName, _libRepo, _presenter);

            case ViewDeskCommand c:
                return ViewDesk.Execute(c.DeskId, c.LibraryName, c.SortSequence, _libRepo, _presenter);

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
                return ListLoans.Execute(c.ItemId, c.LibraryName, _libRepo, _presenter);

            case ReturnItemCommand c:
                return ReturnItem.Execute(c.ItemId, c.DeskId, c.LibraryName, authUser.Id, _libRepo);

            case ListLibrariesCommand c:
                return ListLibs.Execute(_libRepo, _presenter);

            case ListItemsCommand c:
                return ListItems.Execute(c.LibraryName, c.FilterSequence, c.SortSequence, c.ItemName, _libRepo, _presenter);

            case GetLibraryLayoutCommand c:
                return GetLibraryLayout.Execute(c.LibraryName, _libRepo, _presenter);

            case SetLibraryLayoutCommand c:
                return SetLibraryLayout.Execute(c.LibraryName, c.LayoutString, _libRepo, _presenter);

            case FetchFilesCommand c:
                return FetchFiles.Execute(c.LibraryName, _libRepo);
            
            case RebalanceLocalStorageCommand c:
                return RebalanceLocalStorage.Execute(c.LibraryName, _libRepo);

            case NewPropertyCommand c:
                return NewProperty.Execute(c.LibName, c.PropName, c.PropType, _libRepo);

            case ListPropertiesCommand c:
                return ListProperties.Execute(c.LibName, c.ItemId, _libRepo, _presenter);
            
            case SetPropertyCommand c:
                return SetProperty.Execute(c.PropName, c.NewValue, c.LibName, c.ItemId, _libRepo);
            
            case RenamePropertyCommand c:
                return RenameProperty.Execute(c.PropName, c.NewName, c.LibName, _libRepo);

            case UnsetPropertyCommand c:
                return UnsetProperty.Execute(c.PropName, c.TargetValue, c.LibName, c.ItemId, _libRepo);

            case RemovePropertyCommand c:
                return RemoveProperty.Execute(c.PropName, c.LibName, _libRepo);

            case NewNoteCommand c:
                var note = string.IsNullOrWhiteSpace(c.Note)
                    ? _reader.ReadText()
                    : c.Note;

                if (string.IsNullOrWhiteSpace(note))
                    return Result.Fail("Failed to get text input.");

                return NewNote.Execute(c.ItemId, c.LibName, note, authUser, _libRepo);

            case ListNotesCommand c:
                return ListNotes.Execute(c.ItemId, c.LibName, _libRepo, _presenter);

            case EditNoteCommand c:
                return EditNote.Execute(c.ItemId, c.NoteId, c.LibName, _reader, _libRepo);

            case RemoveNoteCommand c:
                return RemoveNote.Execute(c.ItemId, c.NoteId, c.LibName, _libRepo);

            case RemoveLibraryCommand c:
                return RemoveLibrary.Execute(c.Name, _libRepo);

            default:
                return Result.Fail($"Unknown use case: {cmd?.GetType().Name ?? "null"}");
        }
         
    }
}

