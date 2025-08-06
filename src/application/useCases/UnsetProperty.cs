using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Persistence;
using Flexlib.Domain;
using System;
using System.Linq;
using System.Text.Json;

namespace Flexlib.Application.UseCases;

public static class UnsetProperty
{
    public static Result Execute(string propName, string targetValue, string libName, object itemId, ILibraryRepository repo)
    {
        var args = new ParsedArgs(propName, targetValue, libName, itemId, repo);

        var validation = IsOperationAllowed(args);
        if (!validation.IsSuccess)
            return validation;

        return _UnsetProperty(args);
    }

    private static Result _UnsetProperty(ParsedArgs args)
    {
        var lib = args.Repo.GetByName(args.LibName);
        var propertyKey = args.PropName.ToLowerInvariant();
        var selectedItem = lib!.GetItemById(args.ItemId);
        var propertyDef = lib.PropertyDefinitions.FirstOrDefault(d => d.Name == propertyKey);

        if (propertyDef == null)
            return Result.Fail($"Property '{propertyKey}' is not defined in library '{args.LibName}'.");

        if (!selectedItem!.PropertyValues.TryGetValue(propertyKey, out var existingValue))
            return Result.Fail($"Item '{args.ItemId}' does not contain a value for property '{propertyKey}'.");

        if (args.TargetValue == "*")
        {
            selectedItem.PropertyValues[propertyKey] = null;
            args.Repo.Save(lib);
            return Result.Success($"All entries removed from property {propertyKey} for item {selectedItem.Name} in library {lib.Name}.");
        }

        if (propertyDef.IsList)
        {
            List<string> currentList;

            if (existingValue is List<string> list)
            {
                currentList = list;
            }
            else if (existingValue is JsonElement je && je.ValueKind == JsonValueKind.Array)
            {
                currentList = JsonHelpers.JsonElementToStringList(je);
            }
            else
            {
                return Result.Fail($"Property '{propertyKey}' is expected to be a list, but value could not be parsed as a list.");
            }

            bool removed = currentList.RemoveAll(v => v.Equals(args.TargetValue, StringComparison.OrdinalIgnoreCase)) > 0;

            if (!removed)
                return Result.Fail($"Value '{args.TargetValue}' not found in list for property '{propertyKey}'.");

            if (currentList.Any())
                selectedItem.PropertyValues[propertyKey] = currentList;
            else
                selectedItem.PropertyValues[propertyKey] = null;
        }
        else
        {
            string current = existingValue?.ToString() ?? "";
            if (!current.Equals(args.TargetValue, StringComparison.OrdinalIgnoreCase))
                return Result.Fail($"Current value of property '{propertyKey}' does not match '{args.TargetValue}'.");

            selectedItem.PropertyValues[propertyKey] = null;
        }

        args.Repo.Save(lib);

        return Result.Success($"Value '{args.TargetValue}' removed from property '{propertyKey}' in item '{args.ItemId}' of library '{args.LibName}'.");
    }

    private static Result IsOperationAllowed(ParsedArgs args)
    {
        
        if (args.LibName == "Default Library" && AssureDefaultLibrary.Execute(args.Repo).IsFailure)
            return Result.Fail($"Default Library not found.");

        if (string.IsNullOrWhiteSpace(args.PropName.ToLowerInvariant()))
            return Result.Fail("Property name must be provided.");

        if (!string.IsNullOrWhiteSpace(args.LibName))
        {
            var lib = args.Repo.GetByName(args.LibName);
            if (lib == null)
                return Result.Fail($"Library '{args.LibName}' not found.");

            if (!lib.ContainsId(args.ItemId))
                return Result.Fail($"Item '{args.ItemId}' not found in library '{args.LibName}'.");

            var item = lib.GetItemById(args.ItemId);

            if (!item!.PropertyValues.ContainsKey(args.PropName.ToLowerInvariant()))
                return Result.Fail($"Property '{args.PropName.ToLowerInvariant()}' not found in item '{args.ItemId}'.");
        
            var propertyDef = lib.PropertyDefinitions.FirstOrDefault(d => d.Name == args.PropName.ToLowerInvariant());
            if (propertyDef == null)
                return Result.Fail($"Property {args.PropName.ToLowerInvariant()} is not defined in library {args.LibName}.");
        }


        return Result.Success("Operation allowed.");
    }

    public class ParsedArgs
    {
        public string PropName { get; }
        public string TargetValue { get; }
        public string LibName { get; }
        public object ItemId { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(string propName, string targetValue, string libName, object itemId, ILibraryRepository repo)
        {
            PropName = propName;
            TargetValue = targetValue;
            LibName = libName ?? "";
            ItemId = itemId ?? "";
            Repo = repo;
        }
    }
}

