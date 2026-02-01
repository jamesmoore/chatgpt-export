namespace ChatGPTExport.Models
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// For a given conversation, return a list of messages that have content.
        /// </summary>
        /// <param name="conversation">The conversation.</param>
        /// <returns>Enumerable of Messages</returns>
        public static IEnumerable<Message> GetMessagesWithContent(this Conversation conversation)
        {
            return conversation.mapping?
                .Select(p => p.Value?.message)
                .OfType<Message>()
                .Where(m => m.content != null)
                ?? [];
        }

        /// <summary>
        /// For a set of Conversations, extract the most recent version of each conversation.
        /// </summary>
        /// <param name="conversations">Source list of conversations.</param>
        /// <returns>Enumerable of most recent version of each conversation.</returns>
        public static IEnumerable<Conversation> GetLatestConversations(this IEnumerable<Conversations> conversations)
        {
            var mostRecentConversations = conversations.SelectMany(p => p)
                .GroupBy(x => x.conversation_id)
                .OrderBy(group => group.Key)
                .Select(group => group.Where(conv => conv.mapping != null).OrderBy(conv => conv.update_time).Last());
            return mostRecentConversations;
        }
    }
}
