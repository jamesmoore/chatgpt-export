using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatGPTExport.Validators;

namespace ChatGPTExport.Models
{
    internal class ConversationsParser(IFileSystem fileSystem, bool validate)
    {
        public Conversations GetConversations(IFileInfo sourceFile)
        {
            var conversationsJson = fileSystem.File.ReadAllText(sourceFile.FullName);

            var conversations = JsonSerializer.Deserialize<Conversations>(conversationsJson);
            if (conversations == null)
            {
                throw new ApplicationException($"Conversations file {sourceFile.FullName} could not be deserialized");
            }

            if (validate)
            {
                var validateContentTypeResult = ValidateContentTypes(conversationsJson);
                var validateResult = ValidateJsonSerialization(conversationsJson, conversations);

                if (validateContentTypeResult == false || validateResult == false)
                {
                    throw new ValidationException();
                }
            }
            return conversations;
        }

        private static bool ValidateContentTypes(string text)
        {
            var validator = new ContentTypeValidator();
            var unhandled = validator.GetUnhandledContentTypes(text);

            if (unhandled.Any())
            {
                Console.Error.WriteLine("Warning - the following conversations have unsupported content types:");
                foreach (var unhandledContent in unhandled)
                {
                    Console.WriteLine(unhandledContent.Title);
                    foreach (var contentType in unhandledContent.UnhandledContentTypes)
                    {
                        Console.WriteLine("\t" + contentType);
                    }
                }
                return false;
            }
            return true;
        }

        private static readonly JsonSerializerOptions options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Reserialize the deserialized json and compare it to the original, to check everything matches.
        /// </summary>
        /// <param name="originalJson"></param>
        /// <param name="conversations"></param>
        private static bool ValidateJsonSerialization(string originalJson, Conversations conversations)
        {
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
