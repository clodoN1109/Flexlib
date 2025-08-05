using Flexlib.Domain;
using Flexlib.Interface.CLI;
using Flexlib.Interface.Input;
using Flexlib.Infrastructure.Interop;

namespace Flexlib.Application.Ports;


public interface IPresenter
{
    void    Message(string message);
    void    Result(Flexlib.Infrastructure.Interop.Result result);
    void    UserInfo(string userInfo);
    void    ExplainUsage(UsageInfo usageInfo);
    void    AuthStatus(string message);
    void    AuthPrompt(out AuthPromptScreen screen);
    void    RegistrationPrompt(out RegistrationPromptScreen screen);
    void    ShowError(string message);
    void    ListComments(List<Comment> comments, string itemName, string libName);
    void    ListLibs(List<Library> items);
    void    ListItems( 
                List<LibraryItem> items, 
                Library lib, 
                string filterSequence, 
                string sortSequence, 
                double localSizeInBytes, 
                List<string> itemNameFilter);

    void    ListLayoutSequence(List<string> layoutSequence);
    void    AvailableActions(List<string> actions);
    Result  File(string filePath);
    void    ItemProperties(LibraryItem item, Library lib);
    void    LibraryProperties(Library lib);
} 
