using System.IO.Abstractions;
using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    public interface IExporter
    {
        IEnumerable<string> Export(IDirectoryInfo sourceDirectory, Conversation conversation);
        string GetExtension();
    }
}