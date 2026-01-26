using ChatGPTExport.Assets;
using ChatGPTExport.Models;

namespace ChatGPTExport.Formatters
{
    public interface IConversationFormatter
    {
        IEnumerable<string> Format(IAssetLocator assetLocator, Conversation conversation);
        string GetExtension();
    }
}