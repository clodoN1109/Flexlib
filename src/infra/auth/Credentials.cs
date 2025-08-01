using System.Text.Json.Serialization;

namespace Flexlib.Infrastructure.Authentication;


public class AccessCredentials
{
    public string UserId { get; init; } = "";

    [JsonIgnore]
    public string? PlainPassword { get; set; }

    public string? HashedPassword { get; set; }

    public bool IsLegitimate =>
        !string.IsNullOrEmpty(PlainPassword) &&
        !string.IsNullOrEmpty(HashedPassword) &&
        Password.Hash(PlainPassword) == HashedPassword;

}


