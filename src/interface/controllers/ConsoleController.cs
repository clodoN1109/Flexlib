using Flexlib.Common;
using Flexlib.Application.Ports;
using Flexlib.Application.UseCases;
using Flexlib.Infrastructure.Persistence;
using Flexlib.Interface.Input;
using Flexlib.Interface.Output;

namespace Flexlib.Interface.Controllers;

public static class ConsoleController
{

    static Result? result;
    
    private static readonly ILibraryRepository _repo = new JsonLibraryRepository();
    private static readonly IPresenter _presenter = new ConsolePresenter();
    private static readonly IReader _read = new Reader();

    public static void Handle(Command command)
    {

        switch (command)
        {
            case NewLibraryCommand newLib:
                result = NewLibrary.Execute(newLib.Name, newLib.Path, _repo); 
                break;

            case NewItemCommand addItem:
                result = AddItem.Execute(addItem.LibraryName, addItem.ItemOrigin, addItem.ItemName, _repo);
                break;
            
            case ListItemsCommand listItems:
                result = ListItems.Execute(listItems.LibraryName, listItems.FilterSequence, listItems.SortSequence, _repo, _presenter);
                break;

            case GetLibraryLayoutCommand getLayout:
                result = GetLibraryLayout.Execute(getLayout.LibraryName, _repo, _presenter);
                break;
            
            case SetLibraryLayoutCommand setLayout:
                result = SetLibraryLayout.Execute(setLayout.LibraryName, setLayout.LayoutString, _repo, _presenter);
                break;
            
            case RefreshCommand refresh:
                result = Refresh.Execute(refresh.LibraryName, _repo);
                break;
            
            case NewPropertyCommand addProp:
                result = AddProperty.Execute(addProp.LibName, addProp.PropName, addProp.PropType, _repo);
                break;
            
            case ListPropertiesCommand listProps:
                result = ListProperties.Execute(listProps.LibName, listProps.ItemName, _repo);
                break;
            
            case EditPropertyCommand editProp:
                result = EditProperty.Execute(editProp.PropName, editProp.NewValue, editProp.LibName, editProp.ItemName, _repo);
                break;
            
            case NewCommentCommand makeCom:

                if (string.IsNullOrWhiteSpace(makeCom.Comment))
                {
                    makeCom.Comment = (new Reader()).ReadText();
                }

                if (string.IsNullOrWhiteSpace(makeCom.Comment))
                {
                    result = Result.Fail("Failed to get text input.");
                    break;
                }

                result = MakeComment.Execute(makeCom.ItemName, makeCom.LibName, makeCom.Comment, _repo);
                break;

            case ListCommentsCommand listCom:
                result = ListComments.Execute(listCom.ItemName, listCom.LibName, _repo, _presenter);
                break;
            
            case EditCommentCommand editCom:
                result = EditComment.Execute(editCom.ItemName, editCom.CommentId, editCom.LibName, _read, _repo);
                break;
            
            case RemoveLibraryCommand removeLib:
                result = RemoveLibrary.Execute(removeLib.Name, _repo); 
                break;
            
            case UnknownCommand UnknownCmd:
                result = Result.Fail(UnknownCmd.Message);
                break;

            default:
                result = Result.Fail($"Unknown use case {command}.");
                break;
        }

        if (result.IsSuccess)
            _presenter.Success(result.SuccessMessage ?? "");
        else
            _presenter.Failure(result.ErrorMessage ?? "");

    }
}

