using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    public interface IConversationAssetsCache
    {
        IEnumerable<ConversationAssets>? GetConversationAssets();
        void SetConversationAssets(IEnumerable<ConversationAssets> directoryInfos);
    }
}