using Flexlib.Application.Ports;
using Flexlib.Application.UseCases.Common;
using Flexlib.Common;
using Flexlib.Domain;
using System.Text;
using System.Linq;


namespace Flexlib.Application.UseCases;

public static class ListLibs
{
    public static Result Execute(ILibraryRepository repo, IPresenter presenter)
    {
        var parsedArgs = new ParsedArgs(repo, presenter); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _ListLibs(parsedArgs)
            : validation;
    }


    private static Result _ListLibs(ParsedArgs parsedArgs)
    {
        var selectedLibs = parsedArgs.Repo.GetAll();

        if (selectedLibs.Any())
        {
            parsedArgs.Presenter.ListLibs(selectedLibs.ToList());
            return Result.Success($"{selectedLibs.Count()} libraries listed.");
        }
        else
        {
            return Result.Fail("No libraries found.");
        }
    }

    private static Result IsOperationAllowed(ParsedArgs parsedArgs)
    {
        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public ILibraryRepository Repo { get; }
        public IPresenter Presenter { get; }

        public ParsedArgs(ILibraryRepository repo, IPresenter presenter)
        {
            Repo = repo;    
            Presenter = presenter;
        }
    }
}

