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

public static class Router
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

    public static void Route(ParsedInput parsedInput)
    {
        switch (parsedInput)
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
                    _presenter.ExplainUsage(newUserCmd.UsageInstructions());
                }
                return;

            case HelpCommand helpCmd:
                _presenter.ExplainUsage(helpCmd.UsageInstructions());
                return;

            case UnknownCommand unknownCmd:
                _presenter.ExplainUsage(unknownCmd.UsageInstructions());
                return;
        }

        IUser? user;
 
        if (_bypassAuth)
        {
            user = new User(new UntrustedAccessInfo("dev", "dev", "dev"))
            {
                State = UserState.LoggedIn
            };

            _presenter.Message("⚠️  Authentication bypassed (DEBUG mode).");
        }
        else
        {
            user = _authenticator.TryRestoreSession();
            if (user != null)
            {
                _presenter.Message($"Welcome back, {user.Name} (via session)");
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

        switch (parsedInput)
        {
            case Command c:
                if (c.IsValid())
                    ConsoleController.Handle(c, user);
                else
                    _presenter.ExplainUsage(c.UsageInstructions());
                break;

            case GUIStartUp gui:
                if (gui.IsValid())
                {
                    _presenter.Message("Launching Flexlib GUI.");
                    // GUIController.Handle(gui, user);
                }
                break;

            default:
                _presenter.ExplainUsage();
                break;
        }

        _presenter.ExhibitUserInfo(user);
    }
}
