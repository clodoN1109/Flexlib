using System;
using System.Diagnostics;
using System.IO;
using Flexlib.Infrastructure.Interop;

namespace Flexlib.Services.Media;


public class PowerExplorer : IMediaService
{
    public Result TryOpenFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return Result.Fail($"Invalid file path {filePath}.");

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-Command \"ViewFile '{filePath}'\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            return Result.Success($"Opening file {filePath}.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"The [PowerExplorer] service failed to open file {filePath}: {ex.Message}");
        }
    }
}

