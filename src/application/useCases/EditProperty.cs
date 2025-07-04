using Flexlib.Application.Ports;
using Flexlib.Application.UseCases.Common;
using Flexlib.Common;
using Flexlib.Domain;
using System;
using System.Linq;
using System.Text.Json;

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
        
        if (args.LibName == "Default Library" && AssureDefaultLibrary.Execute(args.Repo).IsFailure)
            return Result.Fail($"Default Library not found.");

        if (string.IsNullOrWhiteSpace(args.PropName))
            return Result.Fail("Property name must be provided.");

        // If libName is provided, check that the library exists
        if (!string.IsNullOrWhiteSpace(args.LibName))
        {
            var lib = args.Repo.GetByName(args.LibName);
            if (lib == null)
                return Result.Fail($"Library '{args.LibName}' not found.");

            // If itemName is also provided, check item and property
            if (!string.IsNullOrWhiteSpace(args.ItemName))
            {
                if (!lib.ContainsName(args.ItemName))
                    return Result.Fail($"Item '{args.ItemName}' not found in library '{args.LibName}'.");

                var item = lib.GetItemByName(args.ItemName);

                if (!item!.PropertyValues.ContainsKey(args.PropName))
                    return Result.Fail($"Property '{args.PropName}' not found in item '{args.ItemName}'.");
            }
        }

        return Result.Success("Operation allowed.");
    }

    private static Result ApplyEdit(ParsedArgs args)
    {
        var targetLibraries = string.IsNullOrWhiteSpace(args.LibName)
            ? args.Repo.GetAll()
            : new[] { args.Repo.GetByName(args.LibName)! };

        foreach (var lib in targetLibraries.ToList())
        {
            var propertyDef = lib.PropertyDefinitions.FirstOrDefault(d => d.Name == args.PropName);
            if (propertyDef == null)
                continue; // Skip libraries that don't define the property

            var targetItems = string.IsNullOrWhiteSpace(args.ItemName)
                ? lib.Items
                : new List<LibraryItem> { lib.GetItemByName(args.ItemName)! };

            foreach (var item in targetItems)
            {
                if (!item.PropertyValues.ContainsKey(args.PropName))
                    continue;

                object? existingValue = item.PropertyValues[args.PropName];

                if (propertyDef.IsList)
                {
                    if (existingValue is List<string> existingList)
                    {
                        existingList.Add(args.NewValue.Trim());
                    }
                    else if (existingValue is JsonElement je && je.ValueKind == JsonValueKind.Array)
                    {
                        var list = JsonHelpers.JsonElementToStringList(je);
                        list.Add(args.NewValue.Trim());
                        item.PropertyValues[args.PropName] = list;
                    }
                    else
                    {
                        item.PropertyValues[args.PropName] = new List<string> { args.NewValue.Trim() };
                    }
                }
                else
                {
                    item.PropertyValues[args.PropName] = args.NewValue;
                }
            }

            args.Repo.Save(lib);
        }

        return Result.Success(
            string.IsNullOrWhiteSpace(args.LibName)
                ? $"Property '{args.PropName}' updated for all items in all libraries."
                : string.IsNullOrWhiteSpace(args.ItemName)
                    ? $"Property '{args.PropName}' updated for all items in library '{args.LibName}'."
                    : $"Property '{args.PropName}' updated in item '{args.ItemName}'."
        );
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
            LibName = libName ?? "";
            ItemName = itemName ?? "";
            Repo = repo;
        }
    }
}

