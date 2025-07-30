using Flexlib.Domain;
using Flexlib.Interface.CLI;

namespace Flexlib.Application.Ports;

public interface IPresenter
{
    void ExplainUsage(UsageInfo usageInfo);
    void Success(string message);
    void Failure(string message);
    void ShowError(string message);
    void ListComments(List<Comment> comments, string itemName, string libName);
    void ListLibs(List<Library> items);
    void ListItems(List<LibraryItem> items, Library lib, string filterSequence, string sortSequence, double localSizeInBytes, List<string> itemNameFilter);
    void ListLayoutSequence(List<string> layoutSequence);
    void AvailableActions(List<string> actions);
} 
