using Flexlib.Domain;

namespace Flexlib.Application.Ports;

public interface IPresenter
{
    void ExplainUsage(string? usageInstructions);
    void Success(string message);
    void Failure(string message);
    void ShowError(string message);
    void ListComments(List<Comment> comments);
    void ListLibs(List<Library> items);
    void ListItems(List<LibraryItem> items, Library lib, string filterSequence, string sortSequence);
    void ListLayoutSequence(List<string> layoutSequence);
} 
