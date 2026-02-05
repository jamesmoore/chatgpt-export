using ChatGPTExport.Models;
using ChatGPTExport.Validators;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatGpt.Exporter.Cli.Validators
{
    internal class ConversationsJsonSchemaValidator : IConversationsValidator
    {
        private static readonly JsonSerializerOptions options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Reserialize the deserialized json and compare it to the original, to check everything matches.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="conversations"></param>
        public bool Validate(Stream stream, Conversations conversations)
        {
            stream.Position = 0;
            // for round trip validation of the json schema
            var reserialized = JsonSerializer.Serialize(conversations, options);

            var differences = JsonComparer.CompareJson(stream, reserialized);

            if (differences.Count != 0)
            {
                Console.WriteLine("Found json schema discrepancies.");
                foreach (var diff in differences)
                {
                    Console.WriteLine(diff);
                }
                return false;
            }
            return true;
        }
    }
}
