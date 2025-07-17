using Flexlib.Domain;

namespace Flexlib.Application.Ports;

public interface IReader
{
    string? ReadText(string? initialText);

}
