using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;

namespace Flexlib.Application.UseCases;

public static class FetchFiles
{
    public static Result Execute(string? libName, ILibraryRepository repo)
    {
        Result validation = IsOperationAllowed(libName, repo);

        if (validation.IsSuccess)
        {
            if (libName != null)
            {
                return FetchLibraryFiles(libName, repo);
            }
            else
            {
                List<Result> results = FetchAllLibrariesFiles(repo);
                if (results.Any(r => r.IsFailure))
                    return Result.Fail(string.Join(" | ", results.Select(r => r.ErrorMessage)));
                else
                    return Result.Success("All items from all libraries fetched successfully.");
            }
        }
        else
        {
            return validation;
        }
    }

    private static Result FetchLibraryFiles(string libName, ILibraryRepository repo)
    {
        Library? lib = repo.GetByName(libName);

        if (lib != null)
        {
            return repo.Save(lib);
        }
        else
        {
            return Result.Fail($"Library '{libName}' not found.");
        }
    }

    private static List<Result> FetchAllLibrariesFiles(ILibraryRepository repo)
    {
        List<Result> results = new();

        foreach (Library lib in repo.GetAll())
        {
            results.Add(FetchLibraryFiles(lib.Name!, repo));
        }

        return results;
    }

    private static Result IsOperationAllowed(string? libName, ILibraryRepository repo)
    {
        if (libName?.ToLowerInvariant() == "Default Library".ToLowerInvariant() && AssureDefaultLibrary.Execute(repo).IsFailure)
            return Result.Fail($"Default Library not found.");

        if (libName == null)
            return Result.Success("Operation allowed.");

        Library? selectedLibrary = repo.GetByName(libName);
        if (selectedLibrary == null)
            return Result.Fail($"Library '{libName}' not found.");

        return Result.Success("Operation allowed.");
    }
}

