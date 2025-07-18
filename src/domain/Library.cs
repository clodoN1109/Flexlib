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

    public Result RenderLayout()
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

    public LibraryItem? GetItemByName(string name)
    {
        return Items.FirstOrDefault(i => i.Name == name);
    }

    public LibraryItem? GetItemByOrigin(string origin)
    {
        return Items.FirstOrDefault(i => i.Origin == origin);
    }

    public List<LibraryItem> GetItems(FilterSequence filterSequence, SortSequence sortSequence)
    {        

        object branch = ExtractLayoutBranch(filterSequence);

        List<LibraryItem> selectedItems = FlattenToItemList(branch);

        List<LibraryItem> sortedItems = SortLibraryList(selectedItems, sortSequence);

        return sortedItems;
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
            return new List<LibraryItem>( Items );

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
                        nextLevel[kvp.Key] = kvp.Value;
                }
                else if (dictValue is List<LibraryItem> itemList)
                {
                    if (!nextLevel.ContainsKey("_"))
                        nextLevel["_"] = new List<LibraryItem>();

                    ((List<LibraryItem>)nextLevel["_"]).AddRange(itemList);
                }
            }

            current = nextLevel;
        }

        return current;
    }

    private bool KeyMatchesFilter(string dictKey, List<string> filterValues)
    {
        List<string> keyValues;

        try
        {
            keyValues = JsonSerializer.Deserialize<List<string>>(dictKey) ?? new();
        }
        catch
        {
            keyValues = new List<string> { dictKey };
        }

        return filterValues.Any(fv => keyValues.Contains(fv, StringComparer.OrdinalIgnoreCase));
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

