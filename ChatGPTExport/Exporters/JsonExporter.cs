using System.Text.Json;
using System.Text.Json.Serialization;
using ChatGPTExport.Assets;
using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    internal class JsonExporter : IExporter
    {
        private readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public IEnumerable<string> Export(IAssetLocator assetLocator, Conversation conversation)
        {
            return [JsonSerializer.Serialize(conversation, options)];
        }

        public string GetExtension() => ".json";
    }
}
