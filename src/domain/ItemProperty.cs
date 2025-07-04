using System.Text.Json.Serialization;

namespace Flexlib.Domain;

public class ItemPropertyDefinition
{
    public string Name { get; }
    public string TypeName { get; }

    public ItemPropertyDefinition(string name, string typeName)
    {
        Name = name;
        TypeName = typeName.ToLowerInvariant(); // Normalize for matching
    }

    public Type GetDataType()
    {
        return TypeName switch
        {
            "string"  => typeof(string),
            "integer" => typeof(int),
            "decimal" => typeof(decimal),
            "float"   => typeof(float),
            "bool"    => typeof(bool),
            "list"    => typeof(List<string>),
            _         => throw new ArgumentException($"Unsupported type name: {TypeName}")
        };
    }
    
    [JsonIgnore]
    public bool IsList => TypeName == "list";
}

