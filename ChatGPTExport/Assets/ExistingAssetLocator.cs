using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    public class ExistingAssetLocator(IDirectoryInfo destinationDirectory) : IAssetLocator
    {
        private List<string>? cache = null;
        private readonly IFileSystem fileSystem = destinationDirectory.FileSystem;

        private IEnumerable<string> GetCachedDestinationFiles(string searchPattern)
        {
            var match = GetCache().Where(p => p.Contains(searchPattern));
            return match;
        }

        private List<string> GetCache()
        {
            cache ??= fileSystem.Directory.GetFiles(destinationDirectory.FullName, "*", System.IO.SearchOption.AllDirectories).ToList();
            return cache;
        }

        public void Add(string newFile)
        {
            GetCache().Add(newFile);
        }

        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest)
        {
            var invalidChars = fileSystem.Path.GetInvalidFileNameChars()
                .Concat(new[] { '*', '?', fileSystem.Path.DirectorySeparatorChar, fileSystem.Path.AltDirectorySeparatorChar })
                .Distinct()
                .ToArray();

            if (assetRequest.SearchPattern.IndexOfAny(invalidChars) >= 0)
            {
                return null;
            }

            // it may already exist in the destination directory from a previous export
            var destinationMatches = GetCachedDestinationFiles(assetRequest.SearchPattern).ToList();
            if (destinationMatches.Count == 0)
            {
                destinationMatches = fileSystem.Directory.GetFiles(destinationDirectory.FullName, assetRequest.SearchPattern + "*.*", System.IO.SearchOption.AllDirectories).ToList();
            }

            if (destinationMatches.Count != 0)
            {
                var targetFile = fileSystem.FileInfo.New(destinationMatches.First());
                var relativePath = targetFile.GetRelativePathTo(destinationDirectory);
                if (fileSystem.Path.DirectorySeparatorChar != '/')
                {
                    relativePath = relativePath.Replace(fileSystem.Path.DirectorySeparatorChar, '/');
                }
                return new Asset(targetFile.Name, $"./{Uri.EscapeDataString(relativePath).Replace("%2F", "/")}");
            }
            return null;
        }
    }
}
