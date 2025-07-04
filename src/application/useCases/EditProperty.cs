using Flexlib.Application.Ports;
using Flexlib.Common;
using Flexlib.Domain;
using System;
using System.Linq;

namespace Flexlib.Application.UseCases;

public static class EditProperty
{
    public static Result Execute(string propName, string newValue, string libName, string itemName, ILibraryRepository repo)
    {
        var args = new ParsedArgs(propName, newValue, libName, itemName, repo);

        var validation = IsOperationAllowed(args);
        if (!validation.IsSuccess)
            return validation;

        return ApplyEdit(args);
    }

    private static Result IsOperationAllowed(ParsedArgs args)
    {
        if (string.IsNullOrWhiteSpace(args.LibName))
            return Result.Fail("Library name must be provided.");

        if (string.IsNullOrWhiteSpace(args.ItemName))
            return Result.Fail("Item name must be provided.");

        if (string.IsNullOrWhiteSpace(args.PropName))
            return Result.Fail("Property name must be provided.");

        var lib = args.Repo.GetByName(args.LibName);
        if (lib == null)
            return Result.Fail($"Library '{args.LibName}' not found.");

        if (!lib.ContainsName(args.ItemName))
            return Result.Fail($"Item '{args.ItemName}' not found in library '{args.LibName}'.");

        var item = lib.GetItemByName(args.ItemName)!;
        if (!item.PropertyValues.ContainsKey(args.PropName))
            return Result.Fail($"Property '{args.PropName}' not found in item '{args.ItemName}'.");

        return Result.Success("Operation allowed.");
    }

    private static Result ApplyEdit(ParsedArgs args)
    {
        var lib = args.Repo.GetByName(args.LibName)!;
        var item = lib.GetItemByName(args.ItemName)!;

        var selectedProperty = lib.PropertyDefinitions.FirstOrDefault(def => def.Name == args.PropName);
        if (selectedProperty == null)
        {
            return Result.Fail($"Property '{args.PropName}' is not defined in library '{args.LibName}'.");
        }

        if (selectedProperty.IsList)
        {
            string currentValue = (string) item.PropertyValues[args.PropName];

            var values = currentValue
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .ToList();

            values.Add(args.NewValue.Trim());

            item.PropertyValues[args.PropName] = string.Join(';', values);
        }
        else
        {
            item.PropertyValues[args.PropName] = args.NewValue;
        }

        args.Repo.Save(lib);

        return Result.Success($"Property '{args.PropName}' updated in item '{args.ItemName}'.");
    }

    public class ParsedArgs
    {
        public string PropName { get; }
        public string NewValue { get; }
        public string LibName { get; }
        public string ItemName { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(string propName, string newValue, string libName, string itemName, ILibraryRepository repo)
        {
            PropName = propName;
            NewValue = newValue;
            LibName = libName;
            ItemName = itemName;
            Repo = repo;
        }
    }
}


