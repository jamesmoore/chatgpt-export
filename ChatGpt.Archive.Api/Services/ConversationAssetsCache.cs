using ChatGPTExport;
using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    public class ConversationAssetsCache : IConversationAssetsCache
    {
        private IList<ConversationAssets>? conversationAssets = null;

        public IList<ConversationAssets>? GetConversationAssets()
        {
            return conversationAssets;
        }

        public void SetConversationAssets(IEnumerable<ConversationAssets> directoryInfos)
        {
            this.conversationAssets = directoryInfos.ToList();
        }

        public MediaAssetDefinition? GetMediaAsset(string searchPattern)
        {
            var foundAsset = this.GetConversationAssets().Select((p, i) =>
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

        public string GetMediaAssetPath(int index, string relativePath)
        {
            var parentPath = conversationAssets[index].ParentDirectory;
            return parentPath.FileSystem.Path.Combine(parentPath.FullName, relativePath);
        }
    }
}
