using System.Diagnostics;
using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    public class AssetLocator(IFileSystem fileSystem, IDirectoryInfo directoryInfo, IDirectoryInfo destinationDirectory)
    {
        private List<string>? cachedList = null;

        public string? GetMarkdownImage(string searchPattern, string role, DateTimeOffset? createdDate, DateTimeOffset? updatedDate)
        {
            return FindAssetInSourceDirectory(searchPattern, role, createdDate, updatedDate) ??
                FindAssetInDestinationDirectory(searchPattern + "*.*");
        }

        private IEnumerable<string> GetFiles(string searchPattern)
        {
            cachedList ??= fileSystem.Directory.GetFiles(directoryInfo.FullName, "*", System.IO.SearchOption.AllDirectories).ToList();
            var match = cachedList.Where(p => p.Contains(searchPattern));
            return match;
        }

        private string? FindAssetInSourceDirectory(string searchPattern, string role, DateTimeOffset? createdDate, DateTimeOffset? updatedDate)
        {
            var files = GetFiles(searchPattern).ToList();
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

        private string? FindAssetInDestinationDirectory(string searchPattern)
        {
            // it may already exist in the destination directory from a previous export 
            var destinationMatches = fileSystem.Directory.GetFiles(destinationDirectory.FullName, searchPattern, System.IO.SearchOption.AllDirectories);
            if (destinationMatches.Length != 0)
            {
                var targetFile = fileSystem.FileInfo.New(destinationMatches.First());
                var relativePath = fileSystem.GetRelativePathTo(destinationDirectory, targetFile);
                if (fileSystem.Path.DirectorySeparatorChar != '/')
                {
                    relativePath = relativePath.Replace(fileSystem.Path.DirectorySeparatorChar, '/');
                }
                return $"![{targetFile.Name}](./{relativePath})  ";
            }
            return null;
        }
    }
}
