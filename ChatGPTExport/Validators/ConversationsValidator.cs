using ChatGPTExport.Models;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatGPTExport.Validators
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
        /// <param name="originalJson"></param>
        /// <param name="conversations"></param>
        public bool Validate(Stream originalJson, Conversations conversations)
        {
            originalJson.Position = 0;
            // for round trip validation of the json schema
            var reserialized = JsonSerializer.Serialize(conversations, options);

            var differences = JsonComparer.CompareJson(originalJson, reserialized);

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
