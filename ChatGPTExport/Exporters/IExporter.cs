using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    internal interface IExporter
    {
        void Export(Conversation conversation, string filename);
        string GetExtension();
    }
}