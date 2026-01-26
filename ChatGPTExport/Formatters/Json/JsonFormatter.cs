using System.Text.Json;
using System.Text.Json.Serialization;
using ChatGPTExport.Assets;
using ChatGPTExport.Models;

namespace ChatGPTExport.Formatters.Json
{
    internal class JsonFormatter : IConversationFormatter
    {
        private readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public IEnumerable<string> Format(IAssetLocator assetLocator, Conversation conversation)
        {
            return [JsonSerializer.Serialize(conversation, options)];
        }

        public string GetExtension() => ".json";
    }
}
