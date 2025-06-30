using Flexlib.Domain;
using Flexlib.Application.Common;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases;

public static class UpdateLibrary
{
    
    public static Result Execute()
    {
        return Result.Success("Library updated!");
    }

}
