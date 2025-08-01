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
        if (!cmd.IsValid())
        {
            _presenter.ExplainUsage(cmd.GetUsageInfo());
            return Result.Success("");
        }
        
        if (cmd.IsSpecificHelp())
        {
            _presenter.ExplainUsage(cmd.GetUsageInfo());
            return Result.Success("");
        }

        switch (cmd)
        {
            case NewLibraryCommand c:
                return NewLibrary.Execute(c.Name, c.Path, _libRepo);

            case NewItemCommand c:
                return NewItem.Execute(c.LibraryName, c.ItemOrigin, c.ItemName, _libRepo);

            case RemoveItemCommand c:
                return RemoveItem.Execute(c.ItemId, c.LibraryName, _libRepo);

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

            case NewPropertyCommand c:
                return AddProperty.Execute(c.LibName, c.PropName, c.PropType, _libRepo);

            case ListPropertiesCommand c:
                return ListProperties.Execute(c.LibName, c.ItemName, _libRepo);

            case SetPropertyCommand c:
                return SetProperty.Execute(c.PropName, c.NewValue, c.LibName, c.ItemId, _libRepo);

            case RemovePropertyCommand c:
                return RemoveProperty.Execute(c.PropName, c.LibName, _libRepo);

            case NewCommentCommand c:
                var comment = string.IsNullOrWhiteSpace(c.Comment)
                    ? _reader.ReadText()
                    : c.Comment;

                if (string.IsNullOrWhiteSpace(comment))
                    return Result.Fail("Failed to get text input.");

                return NewComment.Execute(c.ItemId, c.LibName, comment, authUser, _libRepo);

            case ListCommentsCommand c:
                return ListComments.Execute(c.ItemId, c.LibName, _libRepo, _presenter);

            case EditCommentCommand c:
                return EditComment.Execute(c.ItemId, c.CommentId, c.LibName, _reader, _libRepo);

            case RemoveCommentCommand c:
                return RemoveComment.Execute(c.ItemId, c.CommentId, c.LibName, _libRepo);

            case RemoveLibraryCommand c:
                return RemoveLibrary.Execute(c.Name, _libRepo);

            default:
                return Result.Fail($"Unknown use case: {cmd?.GetType().Name ?? "null"}");
        }
         
    }
}

