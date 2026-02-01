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
        private IEnumerable<Conversation>? storedConversations = null;
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
            var conversations = conversationFiles.Select(p => new { 
                ParsedConversations = conversationsParser.GetConversations(p), 
                ParentDirectory = p.Directory
            }).ToList();
            var successfulConversations = conversations.Where(p => p.ParsedConversations.Status == ConversationParseResult.Success && p.ParsedConversations.Conversations != null).ToList();

            var parentDirectories = successfulConversations.OrderByDescending(p => p.ParsedConversations.Conversations!.GetUpdateTime()).Select(p => p.ParentDirectory).ToList();
            
            var latestConversations = successfulConversations.Select(p => p.ParsedConversations.Conversations!).GetLatestConversations();
            return latestConversations;
        }

        public Conversation? GetConversation(string conversationId)
        {
            var conversations = EnsureConversationsPresent();
            var conversation = conversations.FirstOrDefault(p => p.id == conversationId);
            return conversation;
        }
    }
}
