using ChatGPTExport;
using ChatGPTExport.Assets;
using ChatGPTExport.Models;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Services
{
    public class ConversationsService : IConversationsService
    {
        private readonly IOptions<ArchiveSourcesOptions> _options;
        private readonly IFileSystem _fileSystem;
        private readonly IConversationAssetsCache _directoryCache;
        private readonly Lazy<IEnumerable<Conversation>> _storedConversations;

        public ConversationsService(
            IFileSystem fileSystem,
            IConversationAssetsCache directoryCache,
            IOptions<ArchiveSourcesOptions> options)
        {
            _fileSystem = fileSystem;
            _directoryCache = directoryCache;
            _options = options;
            _storedConversations = new Lazy<IEnumerable<Conversation>>(GetConversations, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private IEnumerable<Conversation> EnsureConversationsPresent()
        {
            return _storedConversations.Value;
        }

        public IEnumerable<Conversation> GetLatestConversations()
        {
            var conversations = EnsureConversationsPresent();
            return conversations;
        }

        private IEnumerable<Conversation> GetConversations()
        {
            var directories = _options.Value.SourceDirectories.Select(p => _fileSystem.DirectoryInfo.New(p));
            var conversationFinder = new ConversationFinder();
            var conversationFiles = conversationFinder.GetConversationFiles(directories);
            var conversationsParser = new ConversationsParser([]);
            var conversations = conversationFiles.Select(p => new { 
                ParsedConversations = conversationsParser.GetConversations(p), 
                ParentDirectory = p.Directory
            }).ToList();
            var successfulConversations = conversations.Where(p => p.ParsedConversations.Status == ConversationParseResult.Success && p.ParsedConversations.Conversations != null).ToList();

            var parentDirectories = successfulConversations.OrderByDescending(p => p.ParsedConversations.Conversations!.GetUpdateTime()).Select(p => p.ParentDirectory!).ToList();
            _directoryCache.SetConversationAssets(parentDirectories.Select(ConversationAssets.FromDirectory).ToList());
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
