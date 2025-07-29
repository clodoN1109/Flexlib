using Flexlib.Common;
using Flexlib.Application.Ports;
using Flexlib.Application.UseCases;
using Flexlib.Infrastructure.Persistence;
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

    public static void Handle(Command command, IUser authUser)
    {

        if (Authorization.IsNotAuthorized(command, authUser))
        {
            _presenter.Failure($"User {authUser.Name} is not authorized to action {command.Type}.");
            return;
        }

        var result = Execute(command, authUser);

        if (result.IsSuccess)
            _presenter.Success(result.SuccessMessage ?? "");
        else
            _presenter.Failure(result.ErrorMessage ?? "");
    }

    private static Result Execute(Command command, IUser authUser)
    {
        switch (command)
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
                return ListItems.Execute(c.LibraryName, c.FilterSequence, c.SortSequence, _libRepo, _presenter);

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
                return Result.Fail($"Unknown use case: {command?.GetType().Name ?? "null"}");
        }
    }
}

