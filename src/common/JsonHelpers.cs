using System.Text.Json;
using Flexlib.Domain;

namespace Flexlib.Common;

public static class JsonHelpers
{
    public static void WriteJson(string path, object? content){
        
        File.WriteAllText(path, JsonSerializer.Serialize(content, new JsonSerializerOptions { WriteIndented = true }));
    }
    
    public static List<Library> ReadJson(string json)
    {
        return JsonSerializer.Deserialize<List<Library>>(json) ?? new List<Library>();
    }

    public static List<string> JsonElementToStringList(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
            return new List<string>();

        var list = new List<string>();
        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
                list.Add(item.GetString()!);
            else
                list.Add(item.ToString());
        }

        return list;
    }
 
}
