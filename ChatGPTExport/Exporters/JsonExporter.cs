using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    internal class JsonExporter(IFileSystem fileSystem, IDirectoryInfo destinationDirectory) : IExporter
    {
        private readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public IEnumerable<string> Export(IDirectoryInfo sourceDirectory, Conversation conversation)
        {
            string outputPath = fileSystem.Path.Join(destinationDirectory.FullName);
            return [JsonSerializer.Serialize(conversation, options)];
        }

        public string GetExtension() => ".json";
    }
}
