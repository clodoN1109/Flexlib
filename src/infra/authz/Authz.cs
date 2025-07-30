using Flexlib.Application.Ports;

namespace Flexlib.Infrastructure.Authorization;

public static class Authorization
{
    public static bool IsNotAuthorized(IAction action, IUser user) => false;
    public static bool IsNotAuthorized(string actionName, IUser user) => false;
    
    public static List<string> GetAllAuthorizedActions(IUser user)
    {
        return ActionAccessRules.Rules
            .Where(kvp => user.UserAccessLevel >= kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();
    }
}



