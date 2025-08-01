using Flexlib.Interface.CLI;
using Flexlib.Interface.GUI;
using Flexlib.Interface.Controllers;
using Flexlib.Interface.Input;
using Flexlib.Interface.Output;
using Flexlib.Domain;
using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Persistence;
using Flexlib.Infrastructure.Authentication;
using Flexlib.Infrastructure.Interop;

namespace Flexlib.Interface.Router;

public static class ConsoleRouter
{
    private static readonly IUserRepository _userRepo = new JsonUserRepository();
    private static readonly IPresenter _presenter = new ConsolePresenter();
    private static readonly IReader _reader = new Reader();
    private static readonly Authenticator _auth = new Authenticator(_userRepo, _reader);

    public static void Route(Command cmd)
    {        
        
        switch (cmd)
        {         
            case HelpCommand helpCmd:
                _presenter.ExplainUsage(helpCmd.GetUsageInfo());
                return;
            
            case NewUserCommand newUserCmd:
                _presenter.Auth( _auth.RegisterUser().Message ?? "") ;
                return;
               
            case LoginCommand loginCmd:
                _presenter.Auth( _auth.Login().Message ?? "");
                return;
                    
            case LogoutCommand logoutCmd:
                _presenter.Auth( _auth.Logout().Message ?? "");
                return;
        }

        if (_auth.Authenticate(out IUser user).IsNotLoggedIn)
        {
            _presenter.Auth( "Authentication failed.");
            return;
        }

        ConsoleController.Handle(cmd, user);

    }
}

