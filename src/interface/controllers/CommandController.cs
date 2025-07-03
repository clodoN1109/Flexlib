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

