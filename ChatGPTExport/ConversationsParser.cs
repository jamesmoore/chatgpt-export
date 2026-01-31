using ChatGPTExport.Models;
using ChatGPTExport.Validators;
using System.IO.Abstractions;
using System.Text.Json;

namespace ChatGPTExport
{
    public enum ConversationParseResult
    {
        Success,
        ValidationFail,
        Error,
    }

    public class ConversationsParser(IEnumerable<IConversationsValidator> validators)
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

            if (validators.Any())
            {
                Console.WriteLine($"Validating: {sourceFile.FullName}");
                var results = validators.Select(p => p.Validate(conversationsJsonStream, conversations)).ToList();
                if (results.Any(p => p == false))
                {
                    throw new ValidationException();
                }
            }
            return conversations;
        }
    }
}
