using Flexlib.Application.Ports;

namespace Flexlib.Infrastructure.Authorization;

public static class Authorization
{
    public static bool IsNotAuthorized(IAction action, IUser user) => false;
}
