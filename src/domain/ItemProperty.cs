namespace Flexlib.Domain;

public class ItemPropertyDefinition
{
    public string Name { get; }
    public Type DataType { get; }

    public ItemPropertyDefinition(string name, Type dataType)
    {
        Name = name;
        DataType = dataType;
    }
    
}
