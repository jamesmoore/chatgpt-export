using ChatGPTExport.Models;
using ChatGPTExport.Validators;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatGPTExport
{
    public enum ConversationParseResult
    {
        Success,
        ValidationFail,
        Error,
    }

    internal class ConversationsParser(bool validate)
    {
        public (Conversations? Conversations, ConversationParseResult Status) GetConversations(IFileInfo p)
        {
            try
            {
                Console.WriteLine($"Loading conversation " + p.FullName);
                return (this.GetConversationsForFile(p), ConversationParseResult.Success);
            }
            catch (ValidationException)
            {
                return (null, ConversationParseResult.ValidationFail);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error parsing file: {p.FullName}{Environment.NewLine}\t{ex.Message}");
                return (null, ConversationParseResult.Error);
            }
        }

        private Conversations GetConversationsForFile(IFileInfo sourceFile)
        {
            var conversationsJsonStream = sourceFile.FileSystem.File.OpenRead(sourceFile.FullName);

            var conversations = JsonSerializer.Deserialize<Conversations>(conversationsJsonStream);

            if (conversations == null)
            {
                throw new ApplicationException($"Conversations file {sourceFile.FullName} could not be deserialized");
            }

            if (validate)
            {

                Console.WriteLine($"Validating: {sourceFile.FullName}");
                var validateContentTypeResult = ValidateContentTypes(conversationsJsonStream);
                var validateResult = ValidateJsonSerialization(conversationsJsonStream, conversations);

                if (validateContentTypeResult == false || validateResult == false)
                {
                    throw new ValidationException();
                }
            }
            return conversations;
        }

        private static bool ValidateContentTypes(Stream text)
        {
            text.Position = 0;
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
        private static bool ValidateJsonSerialization(Stream originalJson, Conversations conversations)
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
