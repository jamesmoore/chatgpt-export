using System.IO;
using System.Text.Json;
using ChatGPTExport.Models;

namespace ChatGPTExport.Validators
{
    public class ContentTypeValidator
    {
        public IEnumerable<(string? Title, IEnumerable<string> UnhandledContentTypes)> GetUnhandledContentTypes(Stream json)
        {
            var deserialized = JsonSerializer.Deserialize<Conversations>(json);

            var unhandled = deserialized?.Select(p => new
            {
                p.title,
                unhandled = p.GetUnhandledContentTypes().ToArray()
            }).Where(p => p.unhandled.Length > 0).ToList() ?? [];

            return unhandled.Select(p => (p.title, p.unhandled.AsEnumerable())).ToList();
        }

        private class Conversations : List<Conversation>
        {

        }

        private class Conversation
        {
            public string? title { get; set; }

            public IEnumerable<string> GetUnhandledContentTypes()
            {
                var alltypes = mapping?.Values.Select(p => p.message?.content?.content_type).OfType<string>().Distinct() ?? [];
                var unhandled = alltypes.Except(ContentTypes.AllTypes);
                return unhandled;
            }

            public Dictionary<string, MessageContainer>? mapping { get; set; }
        }

        private class MessageContainer
        {
            public Message? message { get; set; }
        }

        private class Message
        {
            public ContentBase? content { get; set; }
        }

        private class ContentBase
        {
            public string? content_type { get; set; }
        }
    }
}
