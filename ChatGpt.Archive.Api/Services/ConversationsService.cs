using ChatGPTExport;
using ChatGPTExport.Models;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Services
{
    public class ConversationsService(IFileSystem fileSystem, IOptions<ArchiveSourcesOptions> options) : IConversationsService
    {
        private readonly ArchiveSourcesOptions _options = options.Value;
        private readonly IFileSystem fileSystem = fileSystem;
        private IEnumerable<Conversation> storedConversations = null;
        private readonly Lock CreationLock = new();

        private IEnumerable<Conversation> EnsureConversationsPresent()
        {
            if (storedConversations == null)
            {
                lock (CreationLock)
                {
                    storedConversations ??= GetConversations();
                }
            }
            return storedConversations;
        }

        public IEnumerable<Conversation> GetLatestConversations()
        {
            var conversations = EnsureConversationsPresent();
            return conversations;
        }

        private IEnumerable<Conversation> GetConversations()
        {
            var directories = _options.SourceDirectories.Select(p => fileSystem.DirectoryInfo.New(p));
            var conversationFinder = new ConversationFinder();
            var conversationFiles = conversationFinder.GetConversationFiles(directories);
            var conversationsParser = new ConversationsParser([]);
            var conversations = conversationFiles.Select(conversationsParser.GetConversations).ToList();
            var validConversations = conversations.Where(p => p.Status == ConversationParseResult.Success && p.Conversations != null).Select(p => p.Conversations!);
            var latestConversations = validConversations.GetLatestConversations();
            return latestConversations;
        }

        public Conversation GetConversation(string conversationId)
        {
            var conversations = EnsureConversationsPresent();
            var conversation = conversations.FirstOrDefault(p => p.id == conversationId);
            return conversation;
        }
    }
}
