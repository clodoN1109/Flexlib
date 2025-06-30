using Flexlib.Application.Common;
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

            case UpdateLibraryCommand updateLib:
                result = UpdateLibrary.Execute();
                break;

            default:
                result = Result.Fail($"No controller takes the command {command}.");
                break;
        }


        if (result.IsSuccess)
            Output.Success(result.SuccessMessage);
        else
            Output.Failure(result.ErrorMessage);

    }
}

