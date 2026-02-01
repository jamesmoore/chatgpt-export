using ChatGPTExport.Assets;
using ChatGPTExport.Models;
using System.IO.Abstractions;

namespace ChatGPTExport
{
    internal class ExportBootstrap(
        ConversationsParser conversationsParser,
        ExportAssetLocatorFactory exportAssetLocatorFactory,
        ConversationExporter exporter
        )
    {
        public int RunExport(IEnumerable<IFileInfo> conversationFiles, IDirectoryInfo destination)
        {
            var directoryConversationsMap = conversationFiles.Where(p => p.Directory != null)
                .Select(file => (
                    File: file,
                    ParentDirectory: file.Directory!,
                    ConversationParseResult: conversationsParser.GetConversations(file)
                )).ToList();

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
                .Select(p => (Conversations: p.ConversationParseResult.Conversations!, ConversationAssets: ConversationAssets.FromDirectory(p.ParentDirectory)))
                .ToList();

            var conversations = successfulConversations
                .Select(p => p.Conversations)
                .GetLatestConversations()
                .ToList();

            var conversationAssetsList = successfulConversations.OrderByDescending(p => p.Conversations.GetUpdateTime()).Select(p => p.ConversationAssets);
            var assetLocator = exportAssetLocatorFactory.GetAssetLocator(conversationAssetsList, destination);

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
