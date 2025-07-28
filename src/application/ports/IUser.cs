using Flexlib.Application.Common;
using Flexlib.Domain;

namespace Flexlib.Application.Ports;


public interface IUser
{
    string Id { get; }
    string Name { get; set; }
    UserState State { get; set; }
    AccessCredentials Credentials { get; }
}

public enum UserState
{
    LoggedOut,
    LoggedIn,
    Blocked,
    Suspended
}

