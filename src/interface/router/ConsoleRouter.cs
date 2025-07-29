using Flexlib.Interface.CLI;
using Flexlib.Interface.GUI;
using Flexlib.Interface.Controllers;
using Flexlib.Interface.Input;
using Flexlib.Interface.Output;
using Flexlib.Domain;
using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Persistence;
using Flexlib.Infrastructure.Authentication;

namespace Flexlib.Interface.Router;

public static class ConsoleRouter
{
    private static readonly ConsolePresenter _presenter = new ConsolePresenter();
    private static readonly IUserRepository _userRepo = new JsonUserRepository();
    private static readonly IReader _reader = new Reader();
    private static readonly Authenticator _authenticator = new Authenticator(_userRepo, _reader);

#if DEBUG
    private static readonly bool _bypassAuth = true;
#else
    private static readonly bool _bypassAuth = false;
#endif

    public static void Route(Command cmd)
    {
        switch (cmd)
        {
            case NewUserCommand newUserCmd:
                if (newUserCmd.IsValid())
                {
                    var result = _authenticator.RegisterUser();
                    if (result.IsSuccess)
                        _presenter.Success(result?.SuccessMessage ?? "");
                    else
                        _presenter.Failure(result?.ErrorMessage ?? "");
                }
                else
                {
                    _presenter.ExplainUsage(newUserCmd.GetUsageInfo());
                }
                return;

            case HelpCommand helpCmd:
                _presenter.ExplainUsage(helpCmd.GetUsageInfo());
                return;

        }

        IUser? user;
 
        if (_bypassAuth)
        {
            user = new User(new UntrustedAccessInfo("dev", "dev", "dev"))
            {
                State = UserState.LoggedIn
            };

        }
        else
        {
            user = _authenticator.TryRestoreSession();
            if (user != null)
            {
                //_presenter.Message($"Welcome back, {user.Name}!");
            }
            else
            {
                var untrustedUserInfo = AuthPrompt.PromptUserIdentification();
                user = new User(untrustedUserInfo);

                if (_authenticator.IsNotAuthenticated(user))
                {
                    _presenter.Failure("Authentication failed.");
                    return;
                }
            }
        }

        switch (cmd)
        {
            case Command c:
                if (c.IsHelp())
                    _presenter.ExplainUsage(c.GetUsageInfo());
                else if (c.IsValid())
                    ConsoleController.Handle(c, user);
                else
                    _presenter.ExplainUsage(c.GetUsageInfo());
                break;

            default:
                _presenter.ExplainUsage(cmd.GetUsageInfo());
                break;
        }        

        if (_bypassAuth)
        {
            _presenter.Message("⚠  Authentication bypassed (DEBUG mode).");
        }

        _presenter.UserInfo(user);
    
    }
}
