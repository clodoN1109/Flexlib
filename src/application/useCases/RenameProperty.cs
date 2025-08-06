using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Persistence;
using Flexlib.Domain;
using System;
using System.Linq;
using System.Text.Json;

namespace Flexlib.Application.UseCases;

public static class RenameProperty
{
    public static Result Execute(string propName, string newName, string libName, ILibraryRepository repo)
    {
        var args = new ParsedArgs(propName, newName, libName, repo);

        var validation = IsOperationAllowed(args);
        if (!validation.IsSuccess)
            return validation;

        return _RenameProperty(args);
    }

    private static Result _RenameProperty(ParsedArgs args)
    {
        var lib = args.Repo.GetByName(args.LibName);
        if (lib is null)
            return Result.Fail($"Library '{args.LibName}' not found.");

        var oldKey = args.PropName.ToLowerInvariant();
        var newKey = args.NewName.ToLowerInvariant();

        // Step 1: Rename the property definition
        var propDef = lib.PropertyDefinitions.FirstOrDefault(d => d.Name == oldKey);
        if (propDef == null)
            return Result.Fail($"Property '{oldKey}' not defined in library '{args.LibName}'.");

        // Prevent duplicate property definitions
        if (lib.PropertyDefinitions.Any(d => d.Name == newKey))
            return Result.Fail($"Property '{newKey}' is already defined in library '{args.LibName}'.");

        propDef.RenameTo(newKey);

        // Step 2: Update each item's PropertyValues dictionary
        foreach (var item in lib.Items)
        {
            if (item.PropertyValues.TryGetValue(oldKey, out var value))
            {
                // Avoid overwriting an existing value under the new key
                if (item.PropertyValues.ContainsKey(newKey))
                    return Result.Fail($"Item '{item.Id}' already has a property '{newKey}', rename aborted.");

                item.PropertyValues.Remove(oldKey);
                item.PropertyValues[newKey] = value;
            }
        }

        // Step 3: Update the layout sequence if the old property is present
        for (int i = 0; i < lib.LayoutSequence.Count; i++)
        {
            if (lib.LayoutSequence[i].Name == oldKey)
            {
                lib.LayoutSequence[i] = new ItemPropertyDefinition(newKey, propDef.TypeName);
                break;
            }
        }

        lib.RenderLayout();

        args.Repo.Save(lib);

        return Result.Success($"Property '{oldKey}' renamed to '{newKey}' in library '{args.LibName}'.");
    }

    private static Result IsOperationAllowed(ParsedArgs args)
    {
        if (args.LibName == "Default Library" && AssureDefaultLibrary.Execute(args.Repo).IsFailure)
            return Result.Fail($"Default Library not found.");

        if (string.IsNullOrWhiteSpace(args.PropName))
            return Result.Fail("Original property name must be provided.");

        if (string.IsNullOrWhiteSpace(args.NewName))
            return Result.Fail("New property name must be provided.");

        var lib = args.Repo.GetByName(args.LibName);
        if (lib == null)
            return Result.Fail($"Library '{args.LibName}' not found.");

        var propDef = lib.PropertyDefinitions.FirstOrDefault(d => d.Name == args.PropName.ToLowerInvariant());
        if (propDef == null)
            return Result.Fail($"Property '{args.PropName}' not defined in library '{args.LibName}'.");

        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public string PropName { get; }
        public string NewName { get; }
        public string LibName { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(string propName, string newName, string libName, ILibraryRepository repo)
        {
            PropName = propName;
            NewName = newName;
            LibName = libName ?? "";
            Repo = repo;
        }
    }
}

