using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Interop;
using Flexlib.Domain;
using System.Text;


namespace Flexlib.Application.UseCases;

public static class GetLibraryLayout
{
    public static Result Execute(string libName, ILibraryRepository repo, IPresenter presenter)
    {
        var parsedArgs = new ParsedArgs(libName, repo, presenter); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _GetLibraryLayout(parsedArgs)
            : validation;
    }

    private static Result _GetLibraryLayout(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName);
        if (selectedLibrary == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' not found.");

        var layoutSequence = selectedLibrary.LayoutSequence
            .Select(def => def.Name)
            .ToList();

        if (layoutSequence.Count > 0)
        { 
            parsedArgs.Presenter.ListLayoutSequence(layoutSequence);
            return Result.Success("");
        }
        else {
            return Result.Fail("Layout sequence is empty.");
        }
    }

    private static Result IsOperationAllowed(ParsedArgs parsedArgs)
    {
        if (parsedArgs.LibName == "Default Library" && AssureDefaultLibrary.Execute(parsedArgs.Repo).IsFailure)
            return Result.Fail($"Default Library not found.");
        
        if (string.IsNullOrWhiteSpace(parsedArgs.LibName))
            return Result.Fail("Library name must be informed.");

        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName);
        if (selectedLibrary == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' not found.");

        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public string LibName { get; }
        public ILibraryRepository Repo { get; }
        public IPresenter Presenter { get; }

        public ParsedArgs(string libName, ILibraryRepository repo, IPresenter presenter)
        {
            LibName = libName;
            Repo = repo;    
            Presenter = presenter;
        }
    }
}

