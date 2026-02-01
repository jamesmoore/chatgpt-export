using ChatGPTExport;
using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    public class ConversationAssetsCache : IConversationAssetsCache
    {
        private IList<ConversationAssets>? conversationAssets = null;

        public void SetConversationAssets(IEnumerable<ConversationAssets> directoryInfos)
        {
            this.conversationAssets = directoryInfos.ToList();
        }

        public MediaAssetDefinition? FindMediaAsset(string searchPattern)
        {
            if (conversationAssets == null)
            {
                return null;
            }

            var foundAsset = conversationAssets.Select((p, i) =>
            (
                Index: i,
                p.ParentDirectory,
                Asset: p.GetAsset(searchPattern)
            )).FirstOrDefault(p => p.Asset != null);

            if (foundAsset.Asset == null)
            {
                return null;
            }

            var asset = foundAsset.Asset!;

            return new MediaAssetDefinition(asset.Name, foundAsset.Index, asset.GetRelativePathTo(foundAsset.ParentDirectory));
        }

        public string? GetMediaAssetPath(int index, string relativePath)
        {
            if (conversationAssets == null || index >= conversationAssets.Count)
            {
                return null;
            }

            var parentPath = conversationAssets[index].ParentDirectory;
            var combinedPath = parentPath.FileSystem.Path.Combine(parentPath.FullName, relativePath);
            var fullPath = parentPath.FileSystem.Path.GetFullPath(combinedPath);
            var parentFullPath = parentPath.FileSystem.Path.GetFullPath(parentPath.FullName);

            // Validate that the resolved path is within the parent directory to prevent path traversal attacks
            if (!fullPath.StartsWith(parentFullPath + parentPath.FileSystem.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
                !fullPath.Equals(parentFullPath, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return fullPath;
        }
    }
}
