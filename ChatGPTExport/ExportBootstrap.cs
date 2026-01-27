using ChatGPTExport.Assets;
using ChatGPTExport.Formatters;
using System.IO;
using System.IO.Abstractions;

namespace ChatGPTExport
{
    internal class ExportBootstrap(
        IFileSystem fileSystem,
        IEnumerable<IConversationFormatter> conversationFormatters,
        ConversationsParser conversationsParser
        )
    {
        public int RunExport(ExportArgs exportArgs)
        {
            var destination = exportArgs.DestinationDirectory;
            var sources = exportArgs.SourceDirectory;

            var exporter = new ConversationExporter(fileSystem, conversationFormatters, exportArgs.ExportMode);

            var existingAssetLocator = new ExistingAssetLocator(fileSystem, destination);
            var conversationFiles = sources.Select(sourceDir => sourceDir.GetFiles(Constants.SearchPattern, SearchOption.AllDirectories)).
                SelectMany(fileInfo => fileInfo, (fileInfo, file) => new { File = file, ParentDirectory = file.Directory }).ToList();

            var directoryConversationsMap = conversationFiles.Where(p => p.ParentDirectory != null)
                .Select(file => new
                {
                    file.File,
                    ParentDirectory = file.ParentDirectory!,
                    Conversations = conversationsParser.GetConversations(file.File),
                }).ToList();

            var failedValidation = directoryConversationsMap.Where(p => p.Conversations.Status == ConversationParseResult.ValidationFail).ToList();
            if (failedValidation.Count != 0)
            {
                foreach (var conversationFile in failedValidation)
                {
                    Console.Error.WriteLine("Invalid conversation json in " + conversationFile.File.FullName);
                }
                return 1;
            }

            var failedToParse = directoryConversationsMap.Where(p => p.Conversations.Status == ConversationParseResult.Error).ToList();
            if (failedToParse.Count != 0)
            {
                Console.Error.WriteLine($"Failed to parse {failedToParse.Count} file(s) due to errors:");
                foreach (var conversationFile in failedToParse)
                {
                    Console.Error.WriteLine($"  - {conversationFile.File.FullName}");
                }
            }

            var successfulConversations = directoryConversationsMap
                .Where(p => p.Conversations.Status == ConversationParseResult.Success)
                .Select(p => new
                {
                    AssetLocator = new AssetLocator(fileSystem, p.ParentDirectory, destination, existingAssetLocator) as IAssetLocator,
                    p.Conversations.Conversations,
                })
                .OrderBy(x => x.Conversations!.GetUpdateTime())
                .ToList();

            var groupedByConversationId = successfulConversations
                .SelectMany(entry => entry.Conversations!, (entry, Conversation) => (entry.AssetLocator, Conversation))
                .GroupBy(x => x.Conversation.conversation_id)
                .OrderBy(p => p.Key).ToList();

            var count = groupedByConversationId.Count;
            var position = 0;
            foreach (var group in groupedByConversationId)
            {
                var percent = (int)(position++ * 100.0 / count);
                ConsoleFeatures.SetProgress(percent);
                exporter.Process(group, destination);
            }

            return 0;
        }
    }
}
