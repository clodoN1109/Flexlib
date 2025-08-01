using Flexlib.Application.Common;
using Flexlib.Infrastructure.Authorization;
using Flexlib.Infrastructure.Authentication;
using Flexlib.Domain;

namespace Flexlib.Application.Ports;


public interface IUser
{
    string Id { get; }
    string Name { get; set; }
    AccessLevel UserAccessLevel { get; set; }
    UserState State { get; set; }
    AccessCredentials Credentials { get; }
    bool IsLoggedIn { get; } 
    bool IsNotLoggedIn { get; } 
}


