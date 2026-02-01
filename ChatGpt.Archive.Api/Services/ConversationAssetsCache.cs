using ChatGPTExport;
using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    public class ConversationAssetsCache : IConversationAssetsCache
    {
        private readonly object _lock = new();
        private IList<ConversationAssets>? conversationAssets = null;

        public void SetConversationAssets(IEnumerable<ConversationAssets> directoryInfos)
        {
            lock (_lock)
            {
                this.conversationAssets = directoryInfos.ToList();
            }
        }

        public MediaAssetDefinition? FindMediaAsset(string searchPattern)
        {
            lock (_lock)
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
        }

        public string? GetMediaAssetPath(int index, string relativePath)
        {
            lock (_lock)
            {
                if (conversationAssets == null || index >= conversationAssets.Count)
                {
                    return null;
                }

                var parentPath = conversationAssets[index].ParentDirectory;
                return parentPath.FileSystem.Path.Combine(parentPath.FullName, relativePath);
            }
        }
    }
}
