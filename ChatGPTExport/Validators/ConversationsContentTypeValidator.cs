using ChatGPTExport.Models;
using System.IO;

namespace ChatGPTExport.Validators
{
    internal class ConversationsContentTypeValidator : IConversationsValidator
    {
        public bool Validate(Stream text, Conversations conversations)
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
    }
}
