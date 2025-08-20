using System.Diagnostics;
using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    public class AssetLocator(
        IFileSystem fileSystem,
        IDirectoryInfo sourceDirectory,
        IDirectoryInfo destinationDirectory,
        ExistingAssetLocator existingAssetLocator
        ) : IAssetLocator
    {
        private List<string>? cachedSourceList = null;

        public string? GetMarkdownImage(AssetRequest assetRequest)
        {
            return FindAssetInSourceDirectory(assetRequest) ??
                existingAssetLocator.GetMarkdownImage(assetRequest);
        }

        private IEnumerable<string> GetCachedSourceFiles(string searchPattern)
        {
            cachedSourceList ??= fileSystem.Directory.GetFiles(sourceDirectory.FullName, "*", System.IO.SearchOption.AllDirectories).ToList();
            var match = cachedSourceList.Where(p => p.Contains(searchPattern));
            return match;
        }

        private string? FindAssetInSourceDirectory(AssetRequest assetRequest)
        {
            var files = GetCachedSourceFiles(assetRequest.SearchPattern).ToList();
            Debug.Assert(files.Count <= 1); // There shouldn't be more than one file.
            var file = files.FirstOrDefault();
            if (file != null)
            {
                var withoutPath = fileSystem.Path.GetFileName(file);
                var assetsPath = $"{assetRequest.Role}-assets";
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

                    if (assetRequest.CreatedDate.HasValue)
                    {
                        fileSystem.File.SetCreationTimeUtcIfPossible(fullAssetPath, assetRequest.CreatedDate.Value.DateTime);
                    }

                    if (assetRequest.UpdatedDate.HasValue)
                    {
                        fileSystem.File.SetLastWriteTimeUtc(fullAssetPath, assetRequest.UpdatedDate.Value.DateTime);
                    }
                }

                return $"![{withoutPath}](./{assetsPath}/{withoutPath})  ";
            }

            return null;
        }
    }
}
