using Flexlib.Infrastructure.Interop;
using Flexlib.Interface.Input.Heuristics;

namespace Flexlib.Infrastructure.Persistence;

public static class CopyHelpers
{
    public static Result TryCopyToLocal(AddressType type, string sourcePath, string targetPath)
    {
        switch (type)
        {
            case AddressType.Url:
                return DownloadFromUrl(sourcePath, targetPath);

            case AddressType.Unc:
            case AddressType.LocalFile:
                return CopyFromFile(sourcePath, targetPath);

            case AddressType.IPAddress:
            case AddressType.Unknown:
            default:
                return Result.Fail($"No method defined to interact with the requested source.");
        }
    }


    private static Result CopyFromFile(string source, string target)
    {
        try
        {
            File.Copy(source, target, overwrite: false);

            return Result.Success($"Successfully copied file.");
        }
        catch (IOException ex)
        {
            // Handle errors (log, retry, etc.)
            return Result.Fail($"Failed to copy file: {ex.Message}");
        }
    }

    private static Result DownloadFromUrl(string url, string target)
    {
        try
        {
            using var client = new HttpClient();
            var data = client.GetByteArrayAsync(url).Result;
            File.WriteAllBytes(target, data);

            return Result.Success($"Successfully downloaded '{url}'.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to download '{url}': {ex.Message}");
        }
    }


}
