using ChatGPTExport.Models;
using ChatGPTExport.Validators;

namespace ChatGpt.Exporter.Cli.Validators
{
    internal class ConversationsContentTypeValidator : IConversationsValidator
    {
        public bool Validate(Stream stream, Conversations conversations)
        {
            stream.Position = 0;
            var validator = new ContentTypeValidator();
            var unhandled = validator.GetUnhandledContentTypes(stream);

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
    }
}
