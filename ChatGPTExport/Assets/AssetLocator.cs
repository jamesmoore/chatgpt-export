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
        private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "system",
            "user",
            "assistant",
            "tool",
            "function"
        };

        public string? GetMarkdownMediaAsset(AssetRequest assetRequest)
        {
            return FindAssetInSourceDirectory(assetRequest) ??
                existingAssetLocator.GetMarkdownMediaAsset(assetRequest);
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
                var sanitizedRole = SanitizeRole(assetRequest.Role);
                var assetsPath = $"{sanitizedRole}-assets";
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

                return $"![{withoutPath}](./{assetsPath}/{Uri.EscapeDataString(withoutPath)})  ";
            }

            return null;
        }

        private string SanitizeRole(string role)
        {
            if (AllowedRoles.Contains(role))
            {
                return role;
            }

            var invalidChars = fileSystem.Path.GetInvalidFileNameChars()
                .Concat(new[]
                {
                    fileSystem.Path.DirectorySeparatorChar,
                    fileSystem.Path.AltDirectorySeparatorChar,
                    '.'
                })
                .ToHashSet();

            var cleaned = new string(role.Where(c => !invalidChars.Contains(c)).ToArray());

            return AllowedRoles.Contains(cleaned) ? cleaned : "unknown";
        }
    }
}
