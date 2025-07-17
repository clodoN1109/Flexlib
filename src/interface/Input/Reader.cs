using Flexlib.Application.Ports;
using Flexlib.Common;
using System.Diagnostics;

namespace Flexlib.Interface.Input;


public class Reader : IReader
{
    public string? ReadText( string? initialText = null)
    {
        string tempPath = Path.GetTempFileName();

        try
        {
            if (!string.IsNullOrEmpty(initialText))
            {
                File.WriteAllText(tempPath, initialText);
            }

            string? editor = Environment.GetEnvironmentVariable("EDITOR");

            if (string.IsNullOrWhiteSpace(editor))
            {
                if (OperatingSystem.IsWindows())
                    editor = "nano";
                else
                    editor = "notepad";
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = editor,
                Arguments = tempPath,
                UseShellExecute = false
            };

            var process = Process.Start(startInfo);
            if (process == null)
                return null;

            process.WaitForExit();

            return File.ReadAllText(tempPath);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}
