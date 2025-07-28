using Flexlib.Application.Ports;
using Flexlib.Application.Common;
using Flexlib.Common;
using Flexlib.Domain;
using System;
using System.Linq;
using System.Text.Json;

namespace Flexlib.Application.UseCases;

public static class SetProperty
{
    public static Result Execute(string propName, string newValue, string libName, object itemId, ILibraryRepository repo)
    {
        var args = new ParsedArgs(propName, newValue, libName, itemId, repo);

        var validation = IsOperationAllowed(args);
        if (!validation.IsSuccess)
            return validation;

        return _SetProperty(args);
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

            if (!lib.ContainsId(args.ItemId))
                return Result.Fail($"Item '{args.ItemId}' not found in library '{args.LibName}'.");

            var item = lib.GetItemById(args.ItemId);

            if (!item!.PropertyValues.ContainsKey(args.PropName))
                return Result.Fail($"Property '{args.PropName}' not found in item '{args.ItemId}'.");
        
            var propertyDef = lib.PropertyDefinitions.FirstOrDefault(d => d.Name == args.PropName);
            if (propertyDef == null)
                return Result.Fail($"Property {args.PropName} is not defined in library {args.LibName}.");
        }


        return Result.Success("Operation allowed.");
    }

    private static Result _SetProperty(ParsedArgs args)
    {
        var lib = args.Repo.GetByName(args.LibName); 

        var propertyDef = lib!.PropertyDefinitions.FirstOrDefault(d => d.Name == args.PropName);

        var selectedItem = lib.GetItemById(args.ItemId);

        object? existingValue = selectedItem!.PropertyValues[args.PropName];

        if (propertyDef!.IsList)
        {
            if (existingValue is List<string> existingList)
            {
                existingList.Add(args.NewValue.Trim());
            }
            else if (existingValue is JsonElement je && je.ValueKind == JsonValueKind.Array)
            {
                var list = JsonHelpers.JsonElementToStringList(je);
                list.Add(args.NewValue.Trim());
                selectedItem.PropertyValues[args.PropName] = list;
            }
            else
            {
                selectedItem.PropertyValues[args.PropName] = new List<string> { args.NewValue.Trim() };
            }
        }
        else
        {
            selectedItem.PropertyValues[args.PropName] = args.NewValue;
        }

        args.Repo.Save(lib);

        return Result.Success( $"Property '{args.PropName}' updated in item '{args.ItemId}' of library '{args.LibName}'.");
    }

    public class ParsedArgs
    {
        public string PropName { get; }
        public string NewValue { get; }
        public string LibName { get; }
        public object ItemId { get; }
        public ILibraryRepository Repo { get; }

        public ParsedArgs(string propName, string newValue, string libName, object itemId, ILibraryRepository repo)
        {
            PropName = propName;
            NewValue = newValue;
            LibName = libName ?? "";
            ItemId = itemId ?? "";
            Repo = repo;
        }
    }
}

