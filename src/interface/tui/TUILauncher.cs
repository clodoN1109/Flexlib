using Flexlib.Interface;
using Flexlib.Interface.Input;
using Flexlib.Infrastructure.Config;
using Flexlib.Infrastructure.Authentication;
using Flexlib.Infrastructure.Persistence;
using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Ports; 
using Flexlib.Interface.TUI;

namespace Flexlib.Interface.TUI;
    
public class TUILauncher
{
    private static readonly IUserRepository _userRepo = new JsonUserRepository();
    private static readonly IReader _reader = new Reader();

    private IUser? User { get; set; }
    private static readonly Authenticator _auth = new Authenticator(_userRepo, _reader);

    private ITUIApp? _app; 
    private bool IsReady { get; set; }

    public Result Prepare(ITUIApp tuiApp)
    {
        if (_auth.Authenticate(out IUser user, out Result authResult).IsNotLoggedIn)
            return Result.Fail("Authentication failed");
        User = user;    
        
        _app = tuiApp;
        IsReady = true;
        return Result.Success("TUI launcher is ready.");
    }

    public Result Launch()
    {
        if (!IsReady || _app == null)
            return Result.Fail("Launcher was not ready to launch.");

        if (User == null || User.IsNotLoggedIn)
            return Result.Warn("TUI instance closed.");
        
        _app.Run(User);

        Console.Clear();

        return Result.Warn("TUI instance closed.");
    }
}

