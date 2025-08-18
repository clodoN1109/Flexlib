using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Interop;
using Flexlib.Domain;
using System.Text;
using System.Linq;


namespace Flexlib.Application.UseCases;

public static class ListLibs
{
    public static Result Execute(ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _ListLibs(parsedArgs)
            : validation;
    }

    private static Result _ListLibs(ParsedArgs parsedArgs)
    {
        var selectedLibs = parsedArgs.Repo.GetAll();
        return Result.Success("", selectedLibs);
    }

    private static Result IsOperationAllowed(ParsedArgs parsedArgs)
    {
        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public ILibraryRepository Repo { get; }
        public ParsedArgs(ILibraryRepository repo)
        {
            Repo = repo;
        }
    }
}

