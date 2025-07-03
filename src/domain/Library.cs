namespace Flexlib.Domain;

public class Library
{
    public string? Name { get; set; }
    public string Path { get; set; }
    public List<ItemPropertyDefinition> PropertyDefinitions { get; set; }
    public List<LibraryItem> Items { get; set; }

    public Library(string? name, string path)
    {
        Name = name;
        Path = path;
        PropertyDefinitions = new();
        Items = new();
    }

    public Library AddItem(string? name, string origin)
    {
        var newItem = new LibraryItem(name, origin, this);

        foreach (var def in PropertyDefinitions)
        {
            newItem.PropertyValues.Add(def.Name, null);
        }

        Items.Add(newItem);

        return this;
    }

    public ItemPropertyDefinition? GetPropertyDefinition(string name)
    {
        return PropertyDefinitions.FirstOrDefault(p => p.Name == name);
    }
    
    public bool isPropertyValueValid(string? propertyName, object? value)
    {
        var def = GetPropertyDefinition(propertyName!);
        
        if (def == null)
            return false;
        
        if (value != null && !def.DataType.IsAssignableFrom(value.GetType()))
            return false;

        return true;
    }

    public void SetPropertyDefinition(string name, Type dataType)
    {
        PropertyDefinitions.Add( new ItemPropertyDefinition( name, dataType) );
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

}
