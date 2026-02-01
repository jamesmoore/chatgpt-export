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
        
        /// <summary>
        /// Gets the full path for a media asset.
        /// </summary>
        /// <param name="index">Index of the parent directory in the conversation assets list.</param>
        /// <param name="relativePath">Relative path to the asset within the parent directory.</param>
        /// <returns>The full path to the asset, or null if the index is invalid or the path escapes the parent directory.</returns>
        string? GetMediaAssetPath(int index, string relativePath);
    }

    public record MediaAssetDefinition(string Name, int RootId, string RelativePath);
}