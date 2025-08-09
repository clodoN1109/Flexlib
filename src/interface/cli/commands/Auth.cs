using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Modelling;
using Flexlib.Infrastructure.Environment;
using Flexlib.Interface.Input.Heuristics;
using System.IO;
using Flexlib.Interface.Input;
using System.Collections.Generic;

namespace Flexlib.Interface.CLI;


public class NewUserCommand : Command
{

    public NewUserCommand(string[] options)
    {
        Options = options;
    }

    public override string Type => "signup";

    public override bool IsValid()
    {
        if (Options.Length < 2)
        {
            return true;
        }

        return false;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "signup",
            Description = "Registers a new user.",
            Group = CommandGroups.Authentication,
            Syntax = "flexlib signup",
            Options = new List<Option>()
        };
    }
}

public class LoginCommand : Command
{

    public LoginCommand(string[] options)
    {
        Options = options;
    }

    public override string Type => "login";

    public override bool IsValid()
    {
        if (Options.Length < 2)
        {
            return true;
        }

        return false;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "login",
            Description = "Login as an existing user.",
            Group = CommandGroups.Authentication,
            Syntax = "flexlib login",
            Options = new List<Option>()
        };
    }
}

public class LogoutCommand : Command
{

    public LogoutCommand(string[] options)
    {
        Options = options;
    }

    public override string Type => "logout";

    public override bool IsValid()
    {
        if (Options.Length < 2)
        {
            return true;
        }

        return false;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "logout",
            Description = "Logout from an opened session.",
            Group = CommandGroups.Authentication,
            Syntax = "flexlib logout",
            Options = new List<Option>()
        };
    }
}

