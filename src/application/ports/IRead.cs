using Flexlib.Domain;

namespace Flexlib.Application.Ports;

public interface IRead
{
    string? ReadText(string? initialText);

}
