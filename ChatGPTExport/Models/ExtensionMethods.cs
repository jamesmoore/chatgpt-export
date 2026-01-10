namespace ChatGPTExport.Models
{
    internal static class ExtensionMethods
    {
        public static IEnumerable<Message> GetMessagesWithContent(this Conversation conversation)
        {
            return conversation.mapping?
                .Select(p => p.Value?.message)
                .OfType<Message>()
                .Where(m => m.content != null)
                ?? [];
        }
    }
}
