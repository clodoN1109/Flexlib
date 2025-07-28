using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Common;
using Flexlib.Domain;
using System.Text;


namespace Flexlib.Application.UseCases;

public static class SetLibraryLayout
{
    public static Result Execute(string libName, string layoutString, ILibraryRepository repo, IPresenter presenter)
    {
        var parsedArgs = new ParsedArgs(libName, layoutString, repo, presenter); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _SetLibraryLayout(parsedArgs)
            : validation;
    }

    private static Result _SetLibraryLayout(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName);
        if (selectedLibrary == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' not found.");

        selectedLibrary.SetLayout(parsedArgs.LayoutString);

        parsedArgs.Repo.Save(selectedLibrary);

        return Result.Success($"'{parsedArgs.LibName}' library layout updated.");
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
        public string LayoutString { get; }
        public ILibraryRepository Repo { get; }
        public IPresenter Presenter { get; }

        public ParsedArgs(string libName, string layoutString, ILibraryRepository repo, IPresenter presenter)
        {
            LibName = libName;
            LayoutString = layoutString;
            Repo = repo;    
            Presenter = presenter;
        }
    }
}

