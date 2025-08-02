using Flexlib.Infrastructure.Interop;
using Flexlib.Interface.Input.Heuristics;

namespace Flexlib.Infrastructure.Persistence;

public static class FileSystem
{
    public static string? FindExistingItemPath(string localDir, string fileName)
    {
        if (!Directory.Exists(localDir))
            return null;

        foreach (var dir in Directory.GetDirectories(localDir))
        {
            var candidate = Path.Combine(dir, fileName);
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    public static string? FindOrCreateAvailableFolder(string localDir, int maxFilesPerFolder, string newFileName)
    {
        try
        {
            // Ensure localDir exists
            Directory.CreateDirectory(localDir);

            // Enumerate valid numeric subfolders
            var subfolders = Directory.GetDirectories(localDir)
                .Select(Path.GetFileName)
                .OfType<string>() // Remove nulls
                .Select(name => int.TryParse(name, out int number) ? (int?)number : null)
                .Where(n => n.HasValue)
                .Select(n => n!.Value)
                .OrderBy(n => n)
                .ToList();

            // Check for existing folders with capacity
            foreach (int folderIndex in subfolders)
            {
                string folderPath = Path.Combine(localDir, folderIndex.ToString());

                // Ensure the folder still exists (it might have been deleted externally)
                if (!Directory.Exists(folderPath))
                    continue;

                var fileCount = Directory.GetFiles(folderPath).Length;

                if (fileCount < maxFilesPerFolder)
                {
                    // Check if the file already exists there
                    string possiblePath = Path.Combine(folderPath, newFileName);
                    if (!File.Exists(possiblePath))
                        return folderPath;
                }
            }

            // Create a new folder with next available index
            int nextIndex = subfolders.Any() ? subfolders.Max() + 1 : 0;
            string newFolderPath = Path.Combine(localDir, nextIndex.ToString());

            Directory.CreateDirectory(newFolderPath);
            return newFolderPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to locate or create a suitable folder: {ex.Message}");
            return null;
        }
    }

    public static string CreateNextSubfolder(string localRoot, int existingCount)
    {
        int index = existingCount;
        string newFolder;

        do
        {
            newFolder = Path.Combine(localRoot, index.ToString());
            index++;
        } while (Directory.Exists(newFolder));

        Directory.CreateDirectory(newFolder);
        return newFolder;
    }

}
