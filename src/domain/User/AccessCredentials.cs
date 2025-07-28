using System.Text.Json.Serialization;

namespace Flexlib.Domain;


public class AccessCredentials

{
    public string UserId { get; init; } = "";
    [JsonIgnore]
    public string? PlainPassword { get; init; } // Only during login
    public string? HashedPassword { get; set; } // Retrieved from repo
}

