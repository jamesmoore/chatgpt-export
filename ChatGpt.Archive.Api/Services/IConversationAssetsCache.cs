using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    public interface IConversationAssetsCache
    {
        /// <summary>
        /// Set the conversation asset paths to be used for asset resolution.
        /// Must be in order of precedence.
        /// </summary>
        /// <param name="conversationAssets"></param>
        void SetConversationAssets(IEnumerable<ConversationAssets> conversationAssets);
        MediaAssetDefinition? FindMediaAsset(string searchPattern);
        string? GetMediaAssetPath(int index, string relativePath);
    }

    public record MediaAssetDefinition(string Name, int RootId, string RelativePath);
}