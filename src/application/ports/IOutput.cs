using Flexlib.Domain;

namespace Flexlib.Application.Ports;

public interface IOutput
{
    void ExplainUsage(string? usageInstructions);
    void Success(string? message);
    void Failure(string? message);
    void ShowError(string message);
    void ListComments(List<Comment> comments);
} 
