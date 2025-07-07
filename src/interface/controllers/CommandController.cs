using Flexlib.Common;
using Flexlib.Application.Ports;
using Flexlib.Application.UseCases;
using Flexlib.Infrastructure.Persistence;

namespace Flexlib.Interface;

public static class CommandController
{

    static Result? result;
    
    private static readonly ILibraryRepository _repo = new JsonLibraryRepository();

    public static void Handle(Command command)
    {

        switch (command)
        {
            case NewLibraryCommand newLib:
                result = NewLibrary.Execute(newLib.Name, newLib.Path, _repo); 
                break;

            case AddItemCommand addItem:
                result = AddItem.Execute(addItem.LibraryName, addItem.ItemOrigin, addItem.ItemName, _repo);
                break;

            case RefreshCommand refresh:
                result = Refresh.Execute(refresh.LibraryName, _repo);
                break;
            
            case AddPropertyCommand addProp:
                result = AddProperty.Execute(addProp.LibName, addProp.PropName, addProp.PropType, _repo);
                break;
            
            case ListPropertiesCommand listProps:
                result = ListProperties.Execute(listProps.LibName, listProps.ItemName, _repo);
                break;
            
            case EditPropertyCommand editProp:
                result = EditProperty.Execute(editProp.PropName, editProp.NewValue, editProp.LibName, editProp.ItemName, _repo);
                break;
            
            case MakeCommentCommand makeCom:

                if (string.IsNullOrWhiteSpace(makeCom.Comment))
                {
                    makeCom.Comment = Read.ReadText();
                }

                if (string.IsNullOrWhiteSpace(makeCom.Comment))
                {
                    result = Result.Fail("Failed to get text input.");
                    break;
                }

                result = MakeComment.Execute(makeCom.ItemName, makeCom.LibName, makeCom.Comment, _repo);
                break;

            case ListCommentsCommand listCom:
                result = ListComments.Execute(listCom.ItemName, listCom.LibName, _repo);
                break;
            
            case EditCommentCommand editCom:
                result = EditComment.Execute(editCom.ItemName, editCom.LibName, _repo);
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
            Output.Success(result.SuccessMessage);
        else
            Output.Failure(result.ErrorMessage);

    }
}

