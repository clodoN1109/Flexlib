using System.Text.Json;
using Flexlib.Domain;

namespace Flexlib.Infrastructure.Persistence.Common;

public static class Json
{
    public static void WriteJson(string path, object? content){
        
        File.WriteAllText(path, JsonSerializer.Serialize(content, new JsonSerializerOptions { WriteIndented = true }));
    }
    
    public static List<Library> ReadJson(string json)
    {
        return JsonSerializer.Deserialize<List<Library>>(json) ?? new List<Library>();
    }
    
}
