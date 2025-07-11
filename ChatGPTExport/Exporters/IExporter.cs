using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    internal interface IExporter
    {
        IEnumerable<string> Export(Conversation conversation);
        string GetExtension();
    }
}