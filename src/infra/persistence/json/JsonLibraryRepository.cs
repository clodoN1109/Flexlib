using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Persistence;
using Flexlib.Infrastructure.Config;
using Flexlib.Infrastructure.Environment;
using Flexlib.Interface.Input.Heuristics;
using Flexlib.Domain;
using System.IO;
using System;
using System.Text.Json;

namespace Flexlib.Infrastructure.Persistence;

public class JsonLibraryRepository : ILibraryRepository
{
    private readonly string _dataDirectory;
    private readonly string _metaFile;
    private List<Library> _cache;
    private string? ExeFolder;
    private string? AppDataFolder;
    private string? AppConfigFolder;
    private FlexlibConfig Config;

    public JsonLibraryRepository()
    {
        ExeFolder = Env.GetExecutingAssemblyLocation();
        if (ExeFolder == null || !Directory.Exists(ExeFolder))
            throw new DirectoryNotFoundException($"Executable file directory not found: {ExeFolder}");

        AppDataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        if (AppDataFolder == null || !Directory.Exists(AppDataFolder))
            throw new DirectoryNotFoundException($"AppData file directory not found: {AppDataFolder}");

        AppConfigFolder = Path.Combine(ExeFolder, "config");

        Config = LoadOrCreateConfig();

        _dataDirectory =  EnsureDataDirectory(ExeFolder, AppDataFolder);
        _metaFile = EnsureMetaFile();
        _cache = LoadCache(_metaFile);

    }

    private List<Library> LoadCache(string metaFile) {
       
        List<Library> cache;

        if (File.Exists(metaFile))
        {
            var json = File.ReadAllText(metaFile);
            cache = JsonHelpers.ReadJson(json) ?? new List<Library>();
        }
        else
        {
            cache = new List<Library>();
        }

        return cache;
    }

    private string EnsureDataDirectory(string exeFolder, string appDataFolder)
    {
        string dataDirectory;

    #if DEBUG
        dataDirectory = Path.Combine(exeFolder, "data");
    #else
        string flexlibDir = Path.Combine(appDataFolder, "Flexlib");
        Directory.CreateDirectory(flexlibDir);
        dataDirectory = Path.Combine(flexlibDir, "data");
    #endif

        Directory.CreateDirectory(dataDirectory);
        return dataDirectory;
    }

    public string GetDataDirectory()
    {
        return _dataDirectory;
    }
    
    private string EnsureMetaFile()
    {
        string? exeFolder = Env.GetExecutingAssemblyLocation();
        
        if (!Directory.Exists(exeFolder))
            throw new DirectoryNotFoundException($"Directory not found: {exeFolder}");

        string metaFile = Path.Combine(_dataDirectory, "libraries.json");

        return metaFile;
    }

    public Result Save(Library lib)
    {
        if ( string.IsNullOrWhiteSpace(lib.Path) )
        {
            lib.Path = _dataDirectory;
        }

        _cache.RemoveAll(l => l.Name?.ToLowerInvariant() == lib.Name?.ToLowerInvariant() && l.Path == lib.Path);
        _cache.Add(lib);
        JsonHelpers.WriteJson(_metaFile, _cache);

        UpdateLibFileStructure(lib);
        UpdateLibMetaFile(lib);

        return UpdateLocalStorage(lib);
       
    }

    private FlexlibConfig LoadOrCreateConfig()
    {
        string configFilePath = Path.Combine(AppConfigFolder!, "FlexlibConfig.json");

        if (!Directory.Exists(AppConfigFolder))
            Directory.CreateDirectory(AppConfigFolder!);

        if (File.Exists(configFilePath))
        {
            try
            {
                string json = File.ReadAllText(configFilePath);
                var config = JsonSerializer.Deserialize<FlexlibConfig>(json);
                if (config != null)
                    return config;
            }
            catch
            {
                // Optionally log or handle corrupted file
            }
        }

        // Fallback to defaults
        var defaultConfig = new FlexlibConfig();
        string defaultJson = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(configFilePath, defaultJson);

        return defaultConfig;
    }

    public Result Save(LibraryItem item, Library lib){
        
        if ( string.IsNullOrWhiteSpace(lib.Path) )
        {
            lib.Path = _dataDirectory;
        }
        
        _cache.FirstOrDefault(l => l.Name?.ToLowerInvariant() == lib.Name?.ToLowerInvariant() && l.Path == lib.Path)?.Items.RemoveAll(i => i.Id == item.Id);
        _cache.FirstOrDefault(l => l.Name?.ToLowerInvariant() == lib.Name?.ToLowerInvariant() && l.Path == lib.Path)?.Items.Add(item);
        JsonHelpers.WriteJson(_metaFile, _cache);

        UpdateLibMetaFile(lib);
        var result = UpdateLocalStorage(item, lib);
        if (result.IsSuccess)
            return Result.Success($"Item {item.Name} was added to library {lib.Name} with ID {item.Id}");
        else
            return Result.Warn($"Item {item.Name} was added to library {lib.Name} with ID {item.Id}, but a local copy could not be retrieved from the origin: {item.Origin}");

    }

    public Result RemoveLibraryByName(string name)
    {
        var selectedLibrary = _cache.FirstOrDefault(l => l.Name?.ToLowerInvariant() == name.ToLowerInvariant());

        if (selectedLibrary == null || string.IsNullOrWhiteSpace(selectedLibrary.Name))
        {
            return Result.Fail("Library can't be removed because it doesn't exist — which, honestly, is already kind of a win.");
        }

        string libraryPath = Path.Combine(selectedLibrary.Path ?? "", selectedLibrary.Name);

        Console.WriteLine($"\nAre you sure you want to delete the library '{name}' at path:\n\n  {libraryPath} ?\n");
        Console.Write("(y/N) > ");
        string? input = Console.ReadLine();

        if (!string.Equals(input, "y", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Fail("Deletion cancelled by user.");
        }

        if (string.IsNullOrWhiteSpace(libraryPath) || !Directory.Exists(libraryPath))
        {
            return Result.Fail($"Path '{libraryPath}' does not exist.");
        }

        // Safety: avoid deleting suspiciously short root-level paths
        if (Path.GetFullPath(libraryPath).Length < 10)
        {
            return Result.Fail("Library path is suspiciously short. Aborting deletion to protect your filesystem.");
        }

        try
        {
            foreach (var file in Directory.GetFiles(libraryPath, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            Directory.Delete(libraryPath, recursive: true);
           
            _cache.RemoveAll(l => l.Name?.ToLowerInvariant() == name.ToLowerInvariant());
            JsonHelpers.WriteJson(_metaFile, _cache);
            
            return Result.Success($"Library '{name}' was successfully removed.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete library local files in '{libraryPath}': {ex.Message}");
        }

    }

    public Result RemoveItem(LibraryItem item, Library lib)
    {
        if (item == null || lib == null)
            return Result.Fail("Invalid library or item.");

        if (string.IsNullOrWhiteSpace(lib.Path) || string.IsNullOrWhiteSpace(lib.Name))
            return Result.Fail("Library path or name is missing.");

        string libraryFolder = Path.Combine(lib.Path, lib.Name);
        if (!Directory.Exists(libraryFolder))
            return Result.Fail($"Library folder '{libraryFolder}' does not exist.");

        string localItemsFolder = Path.Combine(libraryFolder, "items");

        // Build expected file name
        string fileName = $"{item.Id}-{item.Name}{item.FileExtension}";

        // Locate the file inside segmented local/ subfolders
        string? itemDataPath = null;
        foreach (string subdir in Directory.GetDirectories(localItemsFolder))
        {
            string candidatePath = Path.Combine(subdir, fileName);
            if (File.Exists(candidatePath))
            {
                itemDataPath = candidatePath;
                break;
            }
        }

        Console.WriteLine($"\nAre you sure you want to delete the item '{item.Name}' from library '{lib.Name}'?\n\n");
        Console.Write("(y/N) > ");
        string? input = Console.ReadLine();
        if (!string.Equals(input, "y", StringComparison.OrdinalIgnoreCase))
            return Result.Fail("Deletion cancelled.");

        try
        {
            // Remove item data file from disk
            if (itemDataPath != null && File.Exists(itemDataPath))
            {
                File.Delete(itemDataPath);

                // Optional: clean up empty subdir
                string? parentDir = Path.GetDirectoryName(itemDataPath);
                if (parentDir != null && Directory.Exists(parentDir) && !Directory.EnumerateFileSystemEntries(parentDir).Any())
                {
                    Directory.Delete(parentDir);
                }
            }

            // Update library in memory and on disk
            lib.Items.RemoveAll(i => i.Id == item.Id);

            string libFilePath = Path.Combine(libraryFolder, $"{lib.Name}.json");
            JsonHelpers.WriteJson(libFilePath, lib);
            JsonHelpers.WriteJson(_metaFile, _cache);

            Save(lib);

            return Result.Success($"Item '{item.Name}' removed from library '{lib.Name}'.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to remove item '{item.Name}': {ex.Message}");
        }
    }

    private Result UpdateLocalStorage(Library lib)
    {
        List<string> failed = new();

        foreach (LibraryItem item in lib.Items)
        {
            var result = UpdateLocalStorage(item, lib);
            if (result.IsFailure)
                failed.Add($"{item.Name} ID {item.Id}");
        }
        
        if (failed.Count > 0)
            return Result.Fail($"Failed to update: {string.Join(" | ", failed)}");
        else
            return Result.Success($"All items from library {lib.Name} successfully updated.");

    }

    private Result UpdateLocalStorage(LibraryItem item, Library lib)
    {
        string libDir = Path.Combine(lib.Path, lib.Name!);
        string localDir = Path.Combine(libDir, "items");

        Directory.CreateDirectory(localDir);

        string itemId = item.Id!.ToString();
        string itemName = item.Name!;
        string fileExtension = Path.GetExtension(Path.GetFileName(item.Origin!));
        string fileName = $"{itemId}-{itemName}{fileExtension}";

        string? existingPath = FileSystem.FindExistingItemPath(localDir, fileName);

        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + fileExtension);
        AddressType type = AddressAnalysis.GetAddressType(item.Origin!);

        var fetchResult = FetchSystem.TryCopyToLocal(type, item.Origin!, tempPath);
        if (fetchResult.IsFailure)
            return fetchResult;

        if (existingPath != null)
        {
            try
            {
                byte[] existingBytes = File.ReadAllBytes(existingPath);
                byte[] tempBytes = File.ReadAllBytes(tempPath);

                if (existingBytes.SequenceEqual(tempBytes))
                {
                    File.Delete(tempPath);
                    return Result.Success($"Selected library '{lib.Name}' already up to date.");
                }
            }
            catch (Exception ex)
            {
                File.Delete(tempPath);
                return Result.Fail($"Error comparing files: {ex.Message}");
            }
        }

        // Try to find or create a subfolder with room
        string? targetFolder = FileSystem.FindOrCreateAvailableFolder(localDir, Config.MaxFilesPerFolder, fileName);
        if (targetFolder == null)
        {
            File.Delete(tempPath);
            return Result.Fail("Could not find or create a suitable subfolder.");
        }

        string targetPath = Path.Combine(targetFolder, fileName);

        try
        {
            File.Copy(tempPath, targetPath, overwrite: true);
            if (existingPath != null && existingPath != targetPath)
                File.Delete(existingPath);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to copy updated file to local storage: {ex.Message}");
        }
        finally
        {
            File.Delete(tempPath);
        }

        return Result.Success($"Local copy of item '{item.Name}' updated successfully.");
    }

    public Result RenameItem(LibraryItem item, string newName, Library lib)
    {
        string oldName = item.Name!;
        item.Name = newName.Trim();

        try
        {
            Save(lib);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update repository: {ex.Message}");
        }

        var localRenameResult = RenameLocalCopy(item, oldName, lib);
        if (localRenameResult.IsFailure)
            return Result.Fail($"Item renamed, but local copy update failed: {localRenameResult.Message}");

        return Result.Success($"Item '{item.Id}' renamed to '{newName}' in library '{lib.Name}'.");
    }

    private Result RenameLocalCopy(LibraryItem item, string oldName, Library lib)
    {
        string libDir = Path.Combine(lib.Path, lib.Name!);
        string localDir = Path.Combine(libDir, "items");

        Directory.CreateDirectory(localDir);

        string itemId = item.Id!.ToString();
        string fileExtension = Path.GetExtension(Path.GetFileName(item.Origin!));
        string oldFileName = $"{itemId}-{oldName}{fileExtension}";
        string newFileName = $"{itemId}-{item.Name}{fileExtension}";

        string? oldFilePath = FileSystem.FindExistingItemPath(localDir, oldFileName);
        if (oldFilePath == null)
            return Result.Fail($"Local copy for item '{item.Id}' not found with name '{oldFileName}'.");

        string? containingFolder = Path.GetDirectoryName(oldFilePath);
        if (containingFolder == null)
            return Result.Fail($"Could not determine folder for file '{oldFilePath}'.");

        string newFilePath = Path.Combine(containingFolder, newFileName);

        try
        {
            // If the new file already exists, we replace it; otherwise we rename (move)
            if (File.Exists(newFilePath))
            {
                string backupPath = newFilePath + ".bak";
                File.Replace(oldFilePath, newFilePath, backupPath);
                File.Delete(backupPath); // Optional: clean backup after success
            }
            else
            {
                File.Move(oldFilePath, newFilePath);
            }

            return Result.Success($"Local copy renamed from '{oldFileName}' to '{newFileName}'.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to rename local copy: {ex.Message}");
        }
    }

    public string? GetItemLocalCopy(LibraryItem item, Library lib)
    {
        string libDir = Path.Combine(lib.Path, lib.Name!);
        string localDir = Path.Combine(libDir, "items");

        string itemId = item.Id!.ToString();
        string itemName = item.Name!;
        string extension = Path.GetExtension(Path.GetFileName(item.Origin!));
        string fileName = $"{itemId}-{itemName}{extension}";

        foreach (string subdir in Directory.GetDirectories(localDir))
        {
            string candidatePath = Path.Combine(subdir, fileName);
            if (File.Exists(candidatePath))
                return candidatePath;
        }

        return null;
    }

    private void UpdateLibFileStructure(Library lib)
    {   
        string libDir = Path.Combine(lib.Path, $"{lib.Name}");
        string localDir = Path.Combine(libDir, "items/");

        Directory.CreateDirectory(libDir);
        Directory.CreateDirectory(localDir);
 
    }

    private void UpdateLibMetaFile(Library lib)
    {

        string libDir = Path.Combine(lib.Path, $"{lib.Name}");

        string libMetaFile = Path.Combine(libDir, $"{lib.Name}.json");
        JsonHelpers.WriteJson(libMetaFile, lib.Items);
    
    }

    private void DeleteLibMetaFile(Library lib)
    {
        string libDir = Path.Combine(lib.Path, lib.Name!);
        string libMetaFile = Path.Combine(libDir, $"{lib.Name}.json");

        if (File.Exists(libMetaFile))
            File.Delete(libMetaFile);
    }

    private void DeleteAllLocalMetaFiles()
    {
        foreach (var lib in _cache)
        {
            if (lib == null) continue;

            DeleteLibMetaFile(lib);

        }
    }

    public double GetLocalItemFileSizes(List<LibraryItem> items, Library library)
    {
        if (items == null || library == null || string.IsNullOrEmpty(library.Path) || string.IsNullOrEmpty(library.Name))
            return 0;

        string localRoot = Path.Combine(library.Path, library.Name, "items");

        if (!Directory.Exists(localRoot))
            return 0;

        double totalBytes = 0;

        // Get all subdirectories under local recursively
        var allFolders = Directory.GetDirectories(localRoot, "*", SearchOption.AllDirectories);

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item?.Id.ToString()))
                continue;

            string searchPattern = $"{item.Id}-*";

            foreach (var folder in allFolders)
            {
                var matchingFiles = Directory.GetFiles(folder, searchPattern);
                foreach (var file in matchingFiles)
                {
                    try
                    {
                        totalBytes += new FileInfo(file).Length;
                    }
                    catch
                    {
                        // Skip unreadable files
                    }
                }
            }
        }

        return totalBytes;
    }

    public bool Exists(string name) => _cache.Any( l => l.Name?.ToLowerInvariant() == name.ToLowerInvariant());

    public Library? GetByName(string name) => _cache.FirstOrDefault(l => l.Name?.ToLowerInvariant() == name.ToLowerInvariant());

    public IEnumerable<Library> GetAll() => _cache;

    public Result VerifyAndRebalanceLocalStorage()
    {
        foreach (var library in _cache)
        {
            RebalanceLibrary(library);
        }

        return Result.Success("");
    }

    public Result VerifyAndRebalanceLocalStorage(Library lib)
    {
        RebalanceLibrary(lib);
        return Result.Success("");
    }

    private void RebalanceLibrary(Library lib)
    {
        string libDir = Path.Combine(lib.Path!, lib.Name!);
        string localDir = Path.Combine(libDir, "items");

        if (!Directory.Exists(localDir))
            return;

        var subfolders = Directory.GetDirectories(localDir);

        foreach (var folder in subfolders)
        {
            int fileCount = Directory.GetFiles(folder).Length;
            if (fileCount > Config.MaxFilesPerFolder)
            {
                RebalanceFolder(localDir, folder);
            }
        }

        MergeUnderfilledFolders(localDir);
    }

    private void RebalanceFolder(string localRoot, string oversizedFolder)
    {
        var allFolders = Directory.GetDirectories(localRoot).ToList();
        var filesToMove = Directory.GetFiles(oversizedFolder);

        foreach (var file in filesToMove)
        {
            bool moved = false;

            foreach (var folder in allFolders)
            {
                if (folder == oversizedFolder) continue;

                int count = Directory.GetFiles(folder).Length;
                if (count < Config.MaxFilesPerFolder)
                {
                    string fileName = Path.GetFileName(file);
                    string destination = Path.Combine(folder, fileName);

                    if (!File.Exists(destination))
                    {
                        File.Move(file, destination);
                        moved = true;
                        break;
                    }
                }
            }

            if (!moved)
            {
                string newFolder = FileSystem.CreateNextSubfolder(localRoot, allFolders.Count);
                allFolders.Add(newFolder);

                string fileName = Path.GetFileName(file);
                string destination = Path.Combine(newFolder, fileName);
                File.Move(file, destination);
            }
        }

        if (!Directory.GetFiles(oversizedFolder).Any())
        {
            Directory.Delete(oversizedFolder);
        }
    }

    private void MergeUnderfilledFolders(string localDir)
    {
        var folders = Directory.GetDirectories(localDir)
            .OrderBy(path => path)
            .ToList();

        var folderFileLists = folders
            .Select(f => new { Path = f, Files = Directory.GetFiles(f).ToList() })
            .Where(x => x.Files.Count < Config.MaxFilesPerFolder)
            .ToList();

        int i = 0;
        while (i < folderFileLists.Count)
        {
            var target = folderFileLists[i];
            int capacity = Config.MaxFilesPerFolder - target.Files.Count;

            int j = i + 1;
            while (capacity > 0 && j < folderFileLists.Count)
            {
                var donor = folderFileLists[j];
                var filesToMove = donor.Files.Take(capacity).ToList();

                foreach (var file in filesToMove)
                {
                    string fileName = Path.GetFileName(file);
                    string destination = Path.Combine(target.Path, fileName);
                    if (!File.Exists(destination))
                    {
                        File.Move(file, destination);
                        donor.Files.Remove(file);
                        capacity--;
                    }
                }

                // If donor folder becomes empty, remove it
                if (!donor.Files.Any())
                {
                    Directory.Delete(donor.Path);
                    folderFileLists.RemoveAt(j);
                    // Do not increment j
                }
                else
                {
                    j++;
                }
            }

            i++;
        }
    }



}
