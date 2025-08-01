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

    public override string Type => "new-user";

    public override bool IsValid()
    {
        if (Options.Length == 0)
        {
            return true;
        }

        return false;
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "new-user",
            Description = "Creates a new user.",
            Group = CommandGroups.Authentication,
            Syntax = "flexlib new-user",
            Options = new List<CommandOption>()
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
        if (Options.Length == 0)
        {
            return true;
        }

        return false;
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "login",
            Description = "Login as an existing user.",
            Group = CommandGroups.Authentication,
            Syntax = "flexlib login",
            Options = new List<CommandOption>()
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
        if (Options.Length == 0)
        {
            return true;
        }

        return false;
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "logout",
            Description = "Logout from an opened session.",
            Group = CommandGroups.Authentication,
            Syntax = "flexlib logout",
            Options = new List<CommandOption>()
        };
    }
}

