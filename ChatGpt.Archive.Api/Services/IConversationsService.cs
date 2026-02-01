using ChatGPTExport.Models;

namespace ChatGpt.Archive.Api.Services
{
    public interface IConversationsService
    {
        IEnumerable<Conversation> GetLatestConversations();
        Conversation? GetConversation(string conversationId);
    }
}