using Flexlib.Application.Common;
using Flexlib.Application.Ports;
using Flexlib.Common;
using Flexlib.Domain;

namespace Flexlib.Application.UseCases;

public static class AddProperty
{
    public static Result Execute(string libName, string propName, string propType, ILibraryRepository repo)
    {

        var parsedArgs = new ParsedArgs(libName, propName, propType, repo); 

        var validation = IsOperationAllowed(parsedArgs);

        if (validation.IsSuccess)

            return AddProp(parsedArgs);
        
        else{

            return validation;
        
        }

    }

    private static Result AddProp(ParsedArgs parsedArgs)
    {
        if (!string.IsNullOrWhiteSpace(parsedArgs.LibName))
            return AddPropToLibrary(parsedArgs);
        else
            return AddPropToAllLibraries(parsedArgs);
    }

    private static Result AddPropToLibrary(ParsedArgs parsedArgs)
    {
        var lib = parsedArgs.Repo.GetByName(parsedArgs.LibName);
        
        if (lib == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' not found.");

        lib.AddPropertyDefinition(parsedArgs.PropName, parsedArgs.PropType);
        parsedArgs.Repo.Save(lib);

        return Result.Success($"Property '{parsedArgs.PropName}' added to library '{lib.Name}'.");
    }

    private static Result AddPropToAllLibraries(ParsedArgs parsedArgs)
    {
        foreach (var lib in parsedArgs.Repo.GetAll().ToList())
        {
            lib.AddPropertyDefinition(parsedArgs.PropName, parsedArgs.PropType);
            parsedArgs.Repo.Save(lib);
        }

        return Result.Success($"Property '{parsedArgs.PropName}' added to all libraries.");
    }

    private static Result IsOperationAllowed(ParsedArgs parsedArgs)
    {
        
        if (parsedArgs.LibName == "Default Library" && AssureDefaultLibrary.Execute(parsedArgs.Repo).IsFailure)
            return Result.Fail($"Default Library not found.");

        if (string.IsNullOrWhiteSpace(parsedArgs.PropName))
            return Result.Fail("Property name must be provided.");

        if (string.IsNullOrWhiteSpace(parsedArgs.LibName))
            return Result.Success("Operation allowed.");

        var selectedLibrary = parsedArgs.Repo.GetByName(parsedArgs.LibName);
        if (selectedLibrary == null)
            return Result.Fail($"Library '{parsedArgs.LibName}' not found.");

        if (selectedLibrary.HasPropertyDefinition(parsedArgs.PropName))
            return Result.Fail($"Property '{parsedArgs.PropName}' already exists in library '{parsedArgs.LibName}'.");

        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public string LibName { get; }
        public string PropName { get; }
        public string PropType { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(string libName, string propName, string propType, ILibraryRepository repo)
        {
            LibName = libName;
            PropName = propName;
            PropType = propType;
            Repo = repo;
        }
    }
}

