using Flexlib.Application.Ports;
using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;


namespace Flexlib.Application.UseCases;

public static class NewLibraryReport
{
    public static Result Execute(string libName, ILibraryRepository repo)
    {
        var parsedArgs = new ParsedArgs(libName, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        return validation.IsSuccess
            ? _EmitReport(parsedArgs)
            : validation;
    }

    private static Result _EmitReport(ParsedArgs parsedArgs)
    {
        var selectedLib = parsedArgs.Repo.GetByName(parsedArgs.LibName);
        if (selectedLib != null)
        {
            LibraryReport report = new LibraryReport(selectedLib);
            string reportPath = Path.Combine(selectedLib.Path, $"{selectedLib.Name}/report.json");
            return parsedArgs.Repo.Save(report, reportPath);

        }
        return Result.Fail("Report could not be emitted.");
    }

    private static Result IsOperationAllowed(ParsedArgs parsedArgs)
    {
        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public ILibraryRepository Repo { get; }

        public string LibName { get; }
        public ParsedArgs(string libName, ILibraryRepository repo)
        {
            LibName = libName;
            Repo = repo;
        }
    }
}

