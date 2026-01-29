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
                .Select(p => new { Conversations = p.ConversationParseResult.Conversations!, p.ParentDirectory, p.File })
                .ToList();

            var conversations = successfulConversations
                .SelectMany(p => p.Conversations)
                .GroupBy(x => x.conversation_id)
                .OrderBy(p => p.Key)
                .Select(p => p.Where(p => p.mapping != null).OrderBy(p => p.update_time).Last())
                .ToList();

            var parentDirectories = successfulConversations.OrderByDescending(p => p.Conversations.GetUpdateTime()).Select(p => p.ParentDirectory);
            var assetLocator = new ExportAssetLocatorFactory(fileSystem).GetAssetLocator(parentDirectories, destination);

            var count = conversations.Count;
            var position = 0;
            foreach (var conversation in conversations)
            {
                var percent = (int)(position++ * 100.0 / count);
                ConsoleFeatures.SetProgress(percent);
                exporter.Process(conversation, destination, assetLocator);
            }

            return 0;
        }
    }
}
