using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Interop;
using Flexlib.Domain;
using System;
using System.Linq;
using System.Text.Json;

namespace Flexlib.Application.UseCases;

public static class RemoveProperty
{
    public static Result Execute(string propName, string libName, ILibraryRepository repo)
    {
        var args = new ParsedArgs(propName, libName, repo);

        var validation = IsOperationAllowed(args);
        if (!validation.IsSuccess)
            return validation;

        return _RemoveProperty(args);
    }

    private static Result IsOperationAllowed(ParsedArgs args)
    {
        
        if (args.LibName == "Default Library" && AssureDefaultLibrary.Execute(args.Repo).IsFailure)
            return Result.Fail($"Default Library not found.");

        if (string.IsNullOrWhiteSpace(args.PropName))
            return Result.Fail("Property name must be provided.");

        if (!string.IsNullOrWhiteSpace(args.LibName))
        {
            var lib = args.Repo.GetByName(args.LibName);
            if (lib == null)
                return Result.Fail($"Library '{args.LibName}' not found.");
        
            var propertyDef = lib.PropertyDefinitions.FirstOrDefault(d => d.Name == args.PropName);
            if (propertyDef == null)
                return Result.Fail($"Property {args.PropName} cannot be removed since it was not defined in Library '{args.LibName}' in the first place.");
        }   
        
        return Result.Success("Operation allowed.");
    }

    private static Result _RemoveProperty(ParsedArgs args)
    {        

        var selectedLibrary = args.Repo.GetByName(args.LibName)!;
        
        var result = selectedLibrary.RemovePropertyByName(args.PropName);
       
        if (result.IsSuccess)
            args.Repo.Save(selectedLibrary);

        return result;
    }

    public class ParsedArgs
    {
        public string PropName { get; }
        public string LibName { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(string propName, string libName, ILibraryRepository repo)
        {
            PropName = propName;
            LibName = libName ?? "";
            Repo = repo;
        }
    }
}

