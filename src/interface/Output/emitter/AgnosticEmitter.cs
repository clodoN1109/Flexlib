using System;
using System.IO;

namespace Flexlib.Interface.Output;


public class AgnosticEmitter
{
    private readonly TextWriter _writer;

    public AgnosticEmitter(TextWriter? writer = null)
    {
        _writer = writer ?? Console.Out;
    }

    public void Emit(string message)
    {
        _writer.WriteLine(message);
        _writer.Flush(); // Ensure it’s written immediately
    }

    public void Emit(string message, string tag)
    {
        var tagged = $"[{tag.ToUpper()}] {message}";
        Emit(tagged);
    }
}

