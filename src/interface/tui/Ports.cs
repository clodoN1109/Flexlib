using Flexlib.Application.Ports;

namespace Flexlib.Interface.TUI;

public interface ITUIApp
{
    void Run(IUser user);
}