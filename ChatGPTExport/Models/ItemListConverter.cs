using System.Text.Json;
using System.Text.Json.Serialization;
using static ChatGPTExport.Models.ContentMultimodalText;

namespace ChatGPTExport.Models
{
    public class ItemListConverter : JsonConverter<List<ContentMultimodalTextPartsContainer>>
    {
        public override List<ContentMultimodalTextPartsContainer> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new List<ContentMultimodalTextPartsContainer>();

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected StartArray");

            var serializerOptions = new JsonSerializerOptions(options); // clone in case we need to tweak

            reader.Read();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    result.Add(new ContentMultimodalTextPartsContainer { StringValue = reader.GetString() });
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var complex = JsonSerializer.Deserialize<ContentMultimodalTextParts>(ref reader, serializerOptions);
                    result.Add(new ContentMultimodalTextPartsContainer { ObjectValue = complex });
                }
                else
                {
                    throw new JsonException("Unexpected token in mixed array");
                }

                reader.Read();
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, List<ContentMultimodalTextPartsContainer> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var item in value)
            {
                if (item.IsString)
                {
                    writer.WriteStringValue(item.StringValue);
                }
                else if (item.IsObject)
                {
                    JsonSerializer.Serialize(writer, item.ObjectValue, options);
                }
                else
                {
                    writer.WriteNullValue();
                }
            }

            writer.WriteEndArray();
        }
    }
}
