using System.Diagnostics;
using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    public class AssetLocator(
        IFileSystem fileSystem, 
        IDirectoryInfo sourceDirectory, 
        IDirectoryInfo destinationDirectory,
        ExistingAssetLocator existingAssetLocator
        )
    {
        private List<string>? cachedSourceList = null;

        public string? GetMarkdownImage(string searchPattern, string role, DateTimeOffset? createdDate, DateTimeOffset? updatedDate)
        {
            return FindAssetInSourceDirectory(searchPattern, role, createdDate, updatedDate) ??
                existingAssetLocator.FindAssetInDestinationDirectory(searchPattern);
        }

        private IEnumerable<string> GetCachedSourceFiles(string searchPattern)
        {
            cachedSourceList ??= fileSystem.Directory.GetFiles(sourceDirectory.FullName, "*", System.IO.SearchOption.AllDirectories).ToList();
            var match = cachedSourceList.Where(p => p.Contains(searchPattern));
            return match;
        }

        private string? FindAssetInSourceDirectory(string searchPattern, string role, DateTimeOffset? createdDate, DateTimeOffset? updatedDate)
        {
            var files = GetCachedSourceFiles(searchPattern).ToList();
            Debug.Assert(files.Count <= 1); // There shouldn't be more than one file.
            var file = files.FirstOrDefault();
            if (file != null)
            {
                var withoutPath = fileSystem.Path.GetFileName(file);
                var assetsPath = $"{role}-assets";
                var assetsDir = fileSystem.Path.Join(destinationDirectory.FullName, assetsPath);
                if (fileSystem.Directory.Exists(assetsDir) == false)
                {
                    fileSystem.Directory.CreateDirectory(assetsDir);
                }

                var fullAssetPath = fileSystem.Path.Combine(assetsDir, withoutPath);

                if (fileSystem.File.Exists(fullAssetPath) == false)
                {
                    fileSystem.File.Copy(file, fullAssetPath, true);
                    existingAssetLocator.Add(fullAssetPath);

                    if (createdDate.HasValue)
                    {
                        fileSystem.File.SetCreationTimeUtcIfPossible(fullAssetPath, createdDate.Value.DateTime);
                    }

                    if (updatedDate.HasValue)
                    {
                        fileSystem.File.SetLastWriteTimeUtc(fullAssetPath, updatedDate.Value.DateTime);
                    }
                }

                return $"![{withoutPath}](./{assetsPath}/{withoutPath})  ";
            }

            return null;
        }
    }
}
