using System;
using System.Linq;
using System.Text;

namespace Flexlib.Interface.Output;

public static partial class Components
{
    public class WrappedMessage
    {
        public List<ColoredLine> Lines { get; set; } = new();
    }

}

