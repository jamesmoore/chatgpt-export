using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    public interface IConversationAssetsCache
    {
        MediaAssetDefinition? GetMediaAsset(string searchPattern);
        string GetMediaAssetPath(int index, string relativePath);
        void SetConversationAssets(IEnumerable<ConversationAssets> directoryInfos);
    }

    public record MediaAssetDefinition(string Name, int RootId, string RelativePath);
}