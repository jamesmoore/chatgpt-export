using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    internal static class ExtensionMethods
    {
        public static IEnumerable<Message> GetMessagesWithContent(this Conversation conversation)
        {
            return conversation.mapping.Select(p => p.Value).
                Where(p => p.message != null).
                Select(p => p.message).
                Where(p => p.content != null);
        }
    }
}
