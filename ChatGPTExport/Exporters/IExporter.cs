using ChatGPTExport.Assets;
using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    public interface IExporter
    {
        IEnumerable<string> Export(IAssetLocator assetLocator, Conversation conversation);
        string GetExtension();
    }
}