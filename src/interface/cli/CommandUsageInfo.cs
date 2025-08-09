using Flexlib.Interface.Input;

namespace Flexlib.Interface.CLI;

public class CommandUsageInfo : UsageInfo
{
    public CommandGroup Group { get; set; } = new("", "");

}
