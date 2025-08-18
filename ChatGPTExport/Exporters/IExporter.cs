using ChatGPTExport.Assets;
using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    public interface IExporter
    {
        IEnumerable<string> Export(AssetLocator cachedFileSystemWrapper, Conversation conversation);
        string GetExtension();
    }
}