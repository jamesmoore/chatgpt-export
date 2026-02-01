using ChatGPTExport.Assets;
using Markdig.Helpers;
using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Services
{
    /// <summary>
    /// TODO: Obtain real assets and generate API source address.
    /// </summary>
    public class TempAssetLocatons(
        IDirectoryCache directoryCache,
        IFileSystem fileSystem
        ) : IAssetLocator
    {
        // TODO: Somehow inject conversation parent directories
        // NOTE: Source directories != parent directories
        //  * Parent directories contain a valid deserializable conversations.json. 
        //  * Source directories could potentially contain multiple conversations.json or zero.

        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest)
        {
            var foundAsset = directoryCache.GetDirectories().Select(p => ConversationAssets.FromDirectory(p));

            var matchingAssets = foundAsset.Select(p => p.GetAsset(assetRequest.SearchPattern)).Where(p => p != null).FirstOrDefault();

            // TODO recurse source directories that had a valid conversation, find matching asset, generate URL.
            // Need to devise some 2-way-encoding to relate URL to absolute location on disk.
            return new Asset("Some asset", "/asset/whatever.png");
        }
    }

    public class ConversationAssets
    {
        public static ConversationAssets FromDirectory(IDirectoryInfo parentDirectory)
        {
            var files = parentDirectory.FileSystem.Directory.GetFiles(parentDirectory.FullName, "*", SearchOption.AllDirectories);

            return new ConversationAssets(parentDirectory, files);
        }

        private readonly string[] files;

        private ConversationAssets(IDirectoryInfo parentDirectory, string[] files)
        {
            this.files = files;
        }

        public string? GetAsset(string searchPattern)
        {
            return files.FirstOrDefault(p => p.Contains(searchPattern));
        }
    }
}
