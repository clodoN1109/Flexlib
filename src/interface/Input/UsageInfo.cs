namespace Flexlib.Interface.Input;

public class UsageInfo
{
    public List<string> Meta { get; set; } = new();
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Syntax { get; set; } = "";
    public List<Option> Options { get; set; } = new();
    public List<string> Examples { get; set; } = new();
}
