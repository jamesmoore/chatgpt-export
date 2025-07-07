using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    internal class JsonExporter(IFileSystem fileSystem, IDirectoryInfo directoryInfo) : IExporter
    {
        private readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public void Export(Conversation conversation, string filename)
        {
            string outputPath = fileSystem.Path.Join(directoryInfo.FullName, filename);
            fileSystem.File.WriteAllText(outputPath, JsonSerializer.Serialize(conversation, options));

            fileSystem.File.SetCreationTimeUtcIfPossible(outputPath, conversation.GetCreateTime().DateTime);
            fileSystem.File.SetLastWriteTimeUtc(outputPath, conversation.GetUpdateTime().DateTime);
        }

        public string GetExtension() => ".json";
    }
}
