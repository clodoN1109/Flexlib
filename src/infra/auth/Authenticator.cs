using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Interop;
using Flexlib.Domain;
using Flexlib.Interface.Input;
using Flexlib.Interface.Output;
using System;

namespace Flexlib.Infrastructure.Authentication;


public class Authenticator
{

#if DEBUG
    private static readonly bool _bypassAuth = true;
#else
    private static readonly bool _bypassAuth = false;
#endif

    private readonly IUserRepository _userRepo;
    private readonly IReader _reader;
    private static readonly IPresenter _presenter = new ConsolePresenter();
   
    public Authenticator(IUserRepository userRepo, IReader reader)
    {
        _userRepo = userRepo;
        _reader = reader;

    }

    public IUser Authenticate(out IUser user, out Result result)
    {
        if (_bypassAuth)
        {

            result = Result.Warn("Authentication bypassed (DEBUG mode).");
            user = new User(new UntrustedAccessInfo {
                Name = "dev",
                Id = "dev",
                Password = "123456"
            })
            {
                UserAccessLevel = AccessLevel.Dev,
                State = UserState.LoggedIn
            };

            return user;
        }

        if (TryRestoreSession(out user).IsFailure)
        {
            var userInfo = AuthPrompt.PromptUserIdentification();
            user = new User(userInfo);

            if (IsNotAuthenticated(user))
            {
                user = new User("Anonymous")
                {
                    UserAccessLevel = AccessLevel.Public,
                    State = UserState.LoggedOut
                };
            }
            else
            {
                user.State = UserState.LoggedIn;
                user.UserAccessLevel = AccessLevel.User;
            }
        }
         
        result = Result.Success("");
        return user;
    }

    public Result TryRestoreSession(out IUser user)
    {
        user = new User("Anonymous")  // Default fallback
        {
            UserAccessLevel = AccessLevel.Public,
            State = UserState.LoggedOut
        };

        string? sessionId = _userRepo.GetSession().Id; 
        if (string.IsNullOrWhiteSpace(sessionId))
            return Result.Fail("No session ID found.");

        var restoredUser = _userRepo.GetByHashedId(sessionId);
        if (restoredUser == null)
        {
            ClearSession();
            return Result.Fail("User not found for session ID.");
        }

        restoredUser.State = UserState.LoggedIn;
        user = restoredUser;
        return Result.Success("Session restored.");
    }

    public bool IsNotAuthenticated(IUser user)
    {
        var storedUser = _userRepo.Get(user.Id);
        if (storedUser is null || string.IsNullOrWhiteSpace(storedUser.Credentials.HashedPassword))
            return true;

        bool verified = Password.Verify(
            user.Credentials.PlainPassword ?? "",
            storedUser.Credentials.HashedPassword
        );

        if (verified)
        {
            user.State = UserState.LoggedIn;
            user.Name = storedUser.Name;

            SaveSession(user.Id); // ⬅️ Save session on success
        }

        return !verified;
    }

    public void ClearSession()
    {       
        _userRepo.CloseSession();
    }

    private Result SaveSession(string id)
    {
        return _userRepo.SaveSession(id);
    }

    private Result CloseSession()
    {
        return _userRepo.CloseSession();
    }

    public Result RegisterUser()
    {

        UntrustedAccessInfo untrusted = AuthPrompt.PromptUserRegistration();

        string? name = untrusted.Name;
        string? id = untrusted.Id;
        string? untrustedPassword = untrusted.Password; 
        
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(untrustedPassword))
            return Result.Fail("Name, ID and password are required.");

        if (_userRepo.Exists(id))
            return Result.Fail("A user with this ID already exists.");

            string hashedPassword = Password.Hash(untrustedPassword);

        var user = new User(new UntrustedAccessInfo(id, untrustedPassword, name)){ UserAccessLevel = AccessLevel.User };
        user.Credentials.HashedPassword = hashedPassword;

        _userRepo.Save(user);
        SaveSession(id); // ⬅️ Save session here too

        return Result.Success($"User '{name}' registered successfully.");
    }

    public Result Login()
    {
        Logout();

        UntrustedAccessInfo untrusted = AuthPrompt.PromptUserIdentification();

        string? id = untrusted.Id;
        string? untrustedPassword = untrusted.Password; 

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(untrustedPassword))
            return Result.Fail("ID and password are required.");

        if (! _userRepo.Exists(id))
            return Result.Fail("Invalid credentials.");

        var user = _userRepo.Get(id);

        user!.Credentials.PlainPassword = untrustedPassword;

        if (user.Credentials.IsLegitimate) 
            SaveSession(id);

        return Result.Success($"Session opened.");
    }

    public Result Logout()
    {
        
        CloseSession();

        return Result.Success($"Session closed.");
    }
    
}
