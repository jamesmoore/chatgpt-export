using ChatGPTExport.Assets;

namespace ChatGpt.Archive.Api.Services
{
    public class ConversationAssetsCache : IConversationAssetsCache
    {
        private IEnumerable<ConversationAssets>? conversationAssets = null;

        public IEnumerable<ConversationAssets>? GetConversationAssets()
        {
            return conversationAssets;
        }

        public void SetConversationAssets(IEnumerable<ConversationAssets> directoryInfos)
        {
            this.conversationAssets = directoryInfos;
        }
    }
}
