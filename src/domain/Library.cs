using System;
using System.IO;
using Flexlib.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flexlib.Domain;

public class Library
{
    public string? Name { get; set; }
    public string Path { get; set; }
    public List<ItemPropertyDefinition> PropertyDefinitions { get; set; }
    public List<LibraryItem> Items { get; set; }
    public List<ItemPropertyDefinition> LayoutSequence { get; set; }
    [JsonIgnore]
    private object? RenderedLayout { get; set; }

    public Library(string? name, string path)
    {
        Name = name;
        Path = System.IO.Path.GetFullPath(path);
        PropertyDefinitions = new();
        Items = new();
        LayoutSequence = new();
    }

    public Library AddItem(string name, string origin)
    {
        var newItem = new LibraryItem(name, origin, this);

        foreach (var def in PropertyDefinitions)
        {
            AddPropertyToItem(def, newItem);
        }

        Items.Add(newItem);

        return this;
    }

    public int GetHighestItemId() => Items.Any() ? Items.Max(i => i.Id) : 0;
        
    public Result RemovePropertyByName(string propertyName)
    {
        // Remove from property definitions
        PropertyDefinitions.RemoveAll(p => p.Name == propertyName);

        // Remove from items' PropertyValues
        foreach (var item in Items)
        {
            item.PropertyValues?.Remove(propertyName);
        }

        // Remove from layout sequence
        LayoutSequence?.RemoveAll(l => l.Name == propertyName);

        // Recompute layout
        RenderLayout();

        // Verify removal from PropertyDefinitions
        bool stillInDefinitions = PropertyDefinitions.Any(p => p.Name == propertyName);

        // Verify removal from all PropertyValues
        bool stillInAnyItem = Items.Any(item =>
            item.PropertyValues != null &&
            item.PropertyValues.ContainsKey(propertyName));

        // Verify removal from LayoutSequence
        bool stillInLayout = LayoutSequence != null &&
                             LayoutSequence.Any(p => p.Name == propertyName);

        if (!stillInDefinitions && !stillInAnyItem && !stillInLayout)
        {
            return Result.Success($"Property '{propertyName}' fully removed from library '{Name}'.");
        }
        else
        {
            return Result.Fail(
                $"Failed to fully remove property '{propertyName}' from library '{Name}'." +
                (stillInDefinitions ? " Still in PropertyDefinitions." : "") +
                (stillInAnyItem ? " Still in one or more PropertyValues." : "") +
                (stillInLayout ? " Still in LayoutSequence." : "")
            );
        }
    }

    public void AddPropertyDefinition(string propName, string propType)
    {
        var def = new ItemPropertyDefinition(propName, propType);

        PropertyDefinitions.Add(def);

        foreach (LibraryItem item in Items)
        {
            AddPropertyToItem(def, item);
        }
    }

    private void AddPropertyToItem(ItemPropertyDefinition def, LibraryItem item)
    {
        if (!item.PropertyValues.ContainsKey(def.Name))
            item.PropertyValues.Add(def.Name, null);
    }

    public ItemPropertyDefinition? GetPropertyDefinition(string name)
    {
        return PropertyDefinitions.FirstOrDefault(p => p.Name == name);
    }

    public bool HasPropertyDefinition(string propName)
    {
        return PropertyDefinitions.Any(p => p.Name == propName);
    }

    public bool isPropertyValueValid(string propertyName, object? value)
    {
        var def = GetPropertyDefinition(propertyName);

        if (def == null)
            return false;

        if (value != null && !def.GetDataType().IsAssignableFrom(value.GetType()))
            return false;

        return true;
    }

    public bool ContainsOrigin(string origin)
    {
        return Items.Any(i => i.Origin == origin);
    }

    public bool ContainsName(string name)
    {
        return Items.Any(i => i.Name == name);
    }
    
    public bool ContainsId(object id)
    {
        if (id == null)
            return false;

        if (int.TryParse(id.ToString(), out int idValue))
        {
            return Items.Any(i => i.Id == idValue);
        }

        return false;
    }

    public LibraryItem? GetItemByName(string name)
    {
        return Items.FirstOrDefault(i => i.Name == name);
    }

    public LibraryItem? GetItemByOrigin(string origin)
    {
        return Items.FirstOrDefault(i => i.Origin == origin);
    }
    
    public LibraryItem? GetItemById(object id)
    {
        if (int.TryParse(id?.ToString(), out int parsed))
            return Items.FirstOrDefault(i => i.Id == parsed);
        else
            return null;
    }

    public List<LibraryItem> GetItems(FilterSequence filterSequence, SortSequence sortSequence)
    {        

        if ( !RenderLayout().IsSuccess )
            return new List<LibraryItem>();
        
        object branch = ExtractLayoutBranch(filterSequence);

        List<LibraryItem> selectedItems = FlattenToItemList(branch);

        List<LibraryItem> sortedItems = SortLibraryList(selectedItems, sortSequence);

        return sortedItems;
    }

    public Result SetLayout(string layoutSequence)
    {

        List<ItemPropertyDefinition> validSequence = new();

        if ( string.IsNullOrWhiteSpace( layoutSequence ))
        {
            LayoutSequence = validSequence;
            return RenderLayout();
        }

        List<string> layoutPropertyNames = layoutSequence.Split('/').ToList();

        foreach (var propertyName in layoutPropertyNames)
        {
            if (HasPropertyDefinition(propertyName))
            {
                validSequence.Add(GetPropertyDefinition(propertyName)!);
            }
            else
            {
                return Result.Fail($"Invalid property name {propertyName} passed as layout sequence element.");
            }
        }

        LayoutSequence = validSequence;

        return RenderLayout();
    }

    private Result RenderLayout()
    { 

        if (LayoutSequence.Count == 0) 
            RenderedLayout = Items.ToList();

        var root = new Dictionary<string, object>();
        
        var maxDepth = LayoutSequence.Count - 1;

        foreach (var item in Items)
        {
            var current = root;
            for (int i = 0; i <= maxDepth; i++)
            {
                var propDef = LayoutSequence[i];
                var key = item.PropertyValues[propDef.Name]?.ToString() ?? "<null>";

                if (!current.ContainsKey(key))
                {
                    current[key] = (i == maxDepth)
                        ? new List<LibraryItem>()
                        : new Dictionary<string, object>();
                }

                if (i < maxDepth)
                {
                    current = (Dictionary<string, object>) current[key];
                }
                else
                {
                    ( (List<LibraryItem>) current[key]).Add(item);
                }
            }
        }

        RenderedLayout = root;
        return Result.Success($"Layout successfully rendered.");
    }

    private List<LibraryItem> SortLibraryList(List<LibraryItem> items, SortSequence sortSequence)
    {
        if (sortSequence.Elements.Count == 0)
            return items;

        IOrderedEnumerable<LibraryItem>? ordered = null;

        foreach (var key in sortSequence.Elements)
        {
            var def = GetPropertyDefinition(key);
            if (def == null)
                continue;

            Func<LibraryItem, object?> keySelector = item =>
                item.PropertyValues.TryGetValue(def.Name, out var value) ? value : null;

            if (ordered == null)
            {
                ordered = items.OrderBy(keySelector, Comparer<object?>.Default);
            }
            else
            {
                ordered = ordered.ThenBy(keySelector, Comparer<object?>.Default);
            }
        }

        return ordered?.ToList() ?? items;
    }

    private object ExtractLayoutBranch(FilterSequence filterSequence)
    {
        if (filterSequence.Elements.Count == 0)
            return new List<LibraryItem>(Items);

        if (RenderedLayout is not Dictionary<string, object> layout)
            return new List<LibraryItem>();

        object current = layout;

        foreach (var filterElement in filterSequence.Elements)
        {
            if (current is not Dictionary<string, object> currentDict)
                return new List<LibraryItem>();

            var nextLevel = new Dictionary<string, object>();

            foreach (var (dictKey, dictValue) in currentDict)
            {
                if (!KeyMatchesFilter(dictKey, filterElement))
                    continue;

                if (dictValue is Dictionary<string, object> subDict)
                {
                    foreach (var kvp in subDict)
                    {
                        if (nextLevel.TryGetValue(kvp.Key, out var existing))
                        {
                            if (existing is List<LibraryItem> existingList && kvp.Value is List<LibraryItem> newList)
                            {
                                existingList.AddRange(newList);
                            }
                            else if (existing is Dictionary<string, object> existingDict && kvp.Value is Dictionary<string, object> newDict)
                            {
                                foreach (var newItem in newDict)
                                {
                                    existingDict[newItem.Key] = newItem.Value;
                                }

                            }
                            else
                            {
                                nextLevel[kvp.Key] = kvp.Value;
                            }
                        }
                        else
                        {
                            nextLevel[kvp.Key] = kvp.Value;
                        }
                    }
                }
                else if (dictValue is List<LibraryItem> itemList)
                {
                    if (!nextLevel.TryGetValue("_", out var existing))
                    {
                        nextLevel["_"] = new List<LibraryItem>(itemList);
                    }
                    else if (existing is List<LibraryItem> existingList)
                    {
                        existingList.AddRange(itemList);
                    }
                }
            }

            current = nextLevel;
        }

        return current;
    }

    private bool KeyMatchesFilter(string dictKey, List<string> filterValues)
    {
        if (filterValues.Contains("*"))
            return true;

        List<string> keyValues;

        try
        {
            keyValues = JsonSerializer.Deserialize<List<string>>(dictKey) ?? new();
        }
        catch
        {
            keyValues = new List<string> { dictKey };
        }

        return filterValues.Any( fv =>
                keyValues.Any( kv =>
                kv?.IndexOf(fv, StringComparison.OrdinalIgnoreCase) >= 0
            )
        );
    }
    
    private List<LibraryItem> FlattenToItemList(object node)
    {
        var result = new List<LibraryItem>();

        if (node is List<LibraryItem> itemList)
        {
            result.AddRange(itemList);
        }
        else if (node is Dictionary<string, object> dict)
        {
            foreach (var value in dict.Values)
            {
                result.AddRange(FlattenToItemList(value));
            }
        }

        return result;
    }
}

