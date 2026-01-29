using ChatGPTExport.Assets;
using ChatGPTExport.Formatters;
using System.IO.Abstractions;

namespace ChatGPTExport
{
    internal class ExportBootstrap(
        IFileSystem fileSystem,
        IEnumerable<IConversationFormatter> conversationFormatters,
        ConversationsParser conversationsParser
        )
    {
        public int RunExport(ExportArgs exportArgs, IEnumerable<IFileInfo> conversationFiles)
        {
            var destination = exportArgs.DestinationDirectory;

            var exporter = new ConversationExporter(fileSystem, conversationFormatters, exportArgs.ExportMode);

            var directoryConversationsMap = conversationFiles.Where(p => p.Directory != null)
                .Select(file => new
                {
                    File = file,
                    ParentDirectory = file.Directory!,
                    ConversationParseResult = conversationsParser.GetConversations(file),
                }).ToList();

            var failedValidation = directoryConversationsMap.Where(p => p.ConversationParseResult.Status == ConversationParseResult.ValidationFail).ToList();
            if (failedValidation.Count != 0)
            {
                foreach (var conversationFile in failedValidation)
                {
                    Console.Error.WriteLine("Invalid conversation json in " + conversationFile.File.FullName);
                }
                return 1;
            }

            var failedToParse = directoryConversationsMap.Where(p => p.ConversationParseResult.Status == ConversationParseResult.Error).ToList();
            if (failedToParse.Count != 0)
            {
                Console.Error.WriteLine($"Failed to parse {failedToParse.Count} file(s) due to errors:");
                foreach (var conversationFile in failedToParse)
                {
                    Console.Error.WriteLine($"  - {conversationFile.File.FullName}");
                }
            }

            var successfulConversations = directoryConversationsMap
                .Where(p => p.ConversationParseResult.Status == ConversationParseResult.Success)
                .Select(p => new { Conversations = p.ConversationParseResult.Conversations!, p.ParentDirectory, p.File } )
                .OrderBy(x => x.Conversations.GetUpdateTime())
                .ToList();

            var existingAssetLocator = new ExistingAssetLocator(fileSystem, destination);
            var assetLocators = successfulConversations.OrderByDescending(p => p.Conversations.GetUpdateTime()).Select(p => new AssetLocator(fileSystem, p.ParentDirectory, destination, existingAssetLocator) as IAssetLocator).ToList();
            assetLocators.Insert(0, existingAssetLocator);

            var compositeAssetLocator = new CompositeAssetLocator(assetLocators);

            var groupedByConversationId = successfulConversations
                .SelectMany(p => p.Conversations, (entry, conversation) => conversation)
                .GroupBy(x => x.conversation_id)
                .OrderBy(p => p.Key).ToList();

            var count = groupedByConversationId.Count;
            var position = 0;
            foreach (var group in groupedByConversationId)
            {
                var chosenConversation = group.Where(p => p.mapping != null).ToList();
                var percent = (int)(position++ * 100.0 / count);
                ConsoleFeatures.SetProgress(percent);
                exporter.Process(chosenConversation, destination, compositeAssetLocator);
            }

            return 0;
        }
    }
}
