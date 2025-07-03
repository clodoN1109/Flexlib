using Flexlib.Common;

namespace Flexlib.Infrastructure.Persistence.Common;

public static class CopyHelpers
{
    public static void TryCopyToLocal(AddressType type, string sourcePath, string targetPath)
    {
        switch (type)
        {
            case AddressType.Url:
                DownloadFromUrl(sourcePath, targetPath);
                break;

            case AddressType.Unc:
            case AddressType.LocalFile:
                CopyFromFile(sourcePath, targetPath);
                break;

            case AddressType.IPAddress:
            case AddressType.Unknown:
            default:
                break;
        }
    }


    private static void CopyFromFile(string source, string target)
    {
        try
        {
            File.Copy(source, target, overwrite: false);
        }
        catch (IOException ex)
        {
            // Handle errors (log, retry, etc.)
            Console.Error.WriteLine($"Failed to copy file: {ex.Message}");
        }
    }

    private static void DownloadFromUrl(string url, string target)
    {
        try
        {
            using var client = new HttpClient();
            var data = client.GetByteArrayAsync(url).Result;
            File.WriteAllBytes(target, data);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to download '{url}': {ex.Message}");
        }
    }


}
