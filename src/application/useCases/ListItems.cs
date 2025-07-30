using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Interop;
using Flexlib.Domain;
using System.Text;


namespace Flexlib.Application.UseCases;

public static class ListItems
{
    public static Result Execute(string libName, string filterSequenceString, string sortSequenceString, ILibraryRepository repo, IPresenter presenter)
    {
        var parsedArgs = new ParsedArgs(libName, filterSequenceString, sortSequenceString, repo, presenter); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _ListItems(parsedArgs)
            : validation;
    }

    private static Result _ListItems(ParsedArgs parsedArgs)
    {
        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName)!;
        if (selectedLibrary == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' not found.");

        var filterSequence = new FilterSequence( parsedArgs.FilterSequenceString );
        var sortSequence   = new SortSequence( parsedArgs.SortSequenceString );
        
        var selectedItems = selectedLibrary.GetItems(filterSequence, sortSequence); 

        if (selectedItems is not List<LibraryItem>)
            return Result.Fail($"List of items could not be obtained.");

        double localSizeInBytes = parsedArgs.Repo.GetLocalItemFileSizes(selectedItems, selectedLibrary);
        
        parsedArgs.Presenter.ListItems(selectedItems, selectedLibrary, parsedArgs.FilterSequenceString, parsedArgs.SortSequenceString, localSizeInBytes);
        
        return Result.Success($"");

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
        public string FilterSequenceString { get; }
        public string SortSequenceString { get; }
        public ILibraryRepository Repo { get; }
        public IPresenter Presenter { get; }

        public ParsedArgs(string libName, string filterSequenceString, string sortSequenceString, ILibraryRepository repo, IPresenter presenter)
        {
            LibName = libName;
            FilterSequenceString = filterSequenceString;
            SortSequenceString = sortSequenceString;
            Repo = repo;    
            Presenter = presenter;
        }
    }
}

