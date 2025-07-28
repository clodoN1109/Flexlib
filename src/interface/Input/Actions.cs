using Flexlib.Application.Ports;

namespace Flexlib.Interface.Input;


public abstract class Action : ParsedInput, IAction
{
    public abstract string Type { get; }
}

