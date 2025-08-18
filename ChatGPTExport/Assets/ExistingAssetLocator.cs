using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    public class ExistingAssetLocator(IFileSystem fileSystem, IDirectoryInfo destinationDirectory)
    {
        private List<string>? cachedDestinationList = null;
        private IEnumerable<string> GetCachedDestinationFiles(string searchPattern)
        {
            EnsureCacheExists();
            var match = cachedDestinationList.Where(p => p.Contains(searchPattern));
            return match;
        }

        private void EnsureCacheExists()
        {
            cachedDestinationList ??= fileSystem.Directory.GetFiles(destinationDirectory.FullName, "*", System.IO.SearchOption.AllDirectories).ToList();
        }

        public void Add(string newFile)
        {
            EnsureCacheExists();
            cachedDestinationList?.Add(newFile);
        }

        public string? FindAssetInDestinationDirectory(string searchPattern)
        {
            // it may already exist in the destination directory from a previous export 
            var destinationMatches = GetCachedDestinationFiles(searchPattern).ToList();
            if (destinationMatches.Count == 0)
            {
                destinationMatches = fileSystem.Directory.GetFiles(destinationDirectory.FullName, searchPattern + "*.*", System.IO.SearchOption.AllDirectories).ToList();
            }

            if (destinationMatches.Count != 0)
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
