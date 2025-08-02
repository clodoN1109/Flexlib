using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Persistence;

namespace Flexlib.Application.UseCases;


public static class RebalanceLocalStorage
{
    public static Result Execute(string? libName, ILibraryRepository repo)
    {
        var validation = IsOperationAllowed(libName, repo);
        if (validation.IsFailure)
            return validation;

        return string.IsNullOrWhiteSpace(libName)
            ? RebalanceAllLibraries(repo)
            : Rebalance(libName, repo);
    }

    private static Result Rebalance(string libName, ILibraryRepository repo)
    {
        var lib = repo.GetByName(libName);
        if (lib == null)
            return Result.Fail($"Library '{libName}' not found.");

        try
        {
            var result = repo.VerifyAndRebalanceLocalStorage(lib);
            if (result.IsFailure)
                return Result.Fail(result!.ErrorMessage!);

            return Result.Success($"Library '{lib.Name}' rebalanced.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Unexpected error rebalancing '{lib.Name}': {ex.Message}");
        }
    }

    private static Result RebalanceAllLibraries(ILibraryRepository repo)
    {
        var results = new List<string>();
        var errors = new List<string>();

        foreach (var lib in repo.GetAll())
        {
            try
            {
                var result = repo.VerifyAndRebalanceLocalStorage();
                if (result.IsFailure)
                    errors.Add($"{lib.Name}: {result.ErrorMessage}");
                else
                    results.Add(lib.Name!);
            }
            catch (Exception ex)
            {
                errors.Add($"{lib.Name}: {ex.Message}");
            }
        }

        return errors.Any()
            ? Result.Fail($"Some libraries failed to rebalance:\n{string.Join("\n", errors)}")
            : Result.Success($"Rebalanced libraries: {string.Join(", ", results)}");
    }

    private static Result IsOperationAllowed(string? libName, ILibraryRepository repo)
    {
        if (string.IsNullOrWhiteSpace(libName))
            return Result.Success("");

        var selectedLibrary = repo.GetByName(libName);
        return selectedLibrary == null
            ? Result.Fail($"Library '{libName}' not found.")
            : Result.Success("");
    }
}
