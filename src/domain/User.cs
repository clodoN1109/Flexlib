using Flexlib.Application.Common;
using Flexlib.Application.Ports;
using Flexlib.Domain;
using Flexlib.Interface.Input;
using System.Text.Json.Serialization;
using Flexlib.Infrastructure.Authentication;
using Flexlib.Infrastructure.Authorization;


namespace Flexlib.Domain;

public class User : IUser
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "Unknown";
    public AccessLevel UserAccessLevel { get; set; } = AccessLevel.Public;
    public AccessCredentials Credentials { get; set; } = new();

    [JsonIgnore]
    public UserState State { get; set; } = UserState.LoggedOut;

    public User() { } // <-- Required for deserialization

    public User(UntrustedAccessInfo accessInfo)
    {
        Name = accessInfo?.Name ?? "";
        Id = accessInfo?.Id ?? "";
        Credentials = new AccessCredentials
        {
            UserId = Id,
            PlainPassword = accessInfo?.Password ?? ""
        };
    }
    
    public User(string name)
    {
        Name =  name;
        Id = "";
        Credentials = new AccessCredentials
        {
            UserId = "",
            PlainPassword = ""
        };
    }

    [JsonIgnore]
    public bool IsLoggedIn      => State == UserState.LoggedIn;
    [JsonIgnore]
    public bool IsNotLoggedIn   => !IsLoggedIn;
}



