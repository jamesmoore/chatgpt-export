using ChatGPTExport.Assets;
using ChatGPTExport.Exporters;
using ChatGPTExport.Exporters.Html;
using ChatGPTExport.Exporters.Html.Headers;
using ChatGPTExport.Exporters.Html.Template;
using ChatGPTExport.Exporters.Json;
using ChatGPTExport.Exporters.Markdown;
using ChatGPTExport.Models;
using System.IO;
using System.IO.Abstractions;

namespace ChatGPTExport
{
    internal class ExportBootstrap(IFileSystem fileSystem)
    {
        public int RunExport(ExportArgs exportArgs)
        {
            var destination = exportArgs.DestinationDirectory;
            var sources = exportArgs.SourceDirectory;
            var conversationsFactory = new ConversationsParser(fileSystem, exportArgs.Validate);
            var exporters = GetExporters(exportArgs);

            var exporter = new Exporter(fileSystem, exporters, exportArgs.ExportMode);

            var existingAssetLocator = new ExistingAssetLocator(fileSystem, destination);
            var conversationFiles = sources.Select(sourceDir => sourceDir.GetFiles(Constants.SearchPattern, SearchOption.AllDirectories)).
                SelectMany(fileInfo => fileInfo, (fileInfo, file) => new { File = file, ParentDirectory = file.Directory }).ToList();

            var directoryConversationsMap = conversationFiles.Where(p => p.ParentDirectory != null)
                .Select(file => new
                {
                    file.File,
                    file.ParentDirectory,
                    Conversations = conversationsFactory.GetConversations(file.File),
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

            var successfulConversations = directoryConversationsMap
                .Select(p => new{
                    AssetLocator = new AssetLocator(fileSystem, p.ParentDirectory!, destination, existingAssetLocator) as IAssetLocator,
                    p.Conversations.Conversations,
                    p.File,
                    p.ParentDirectory,
                })
                .Where(x => x.Conversations != null)
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

        static IEnumerable<IExporter> GetExporters(ExportArgs exportArgs)
        {
            var exporters = new List<IExporter>();
            if (exportArgs.ExportTypes.Contains(ExportType.Json))
            {
                exporters.Add(new JsonExporter());
            }
            if (exportArgs.ExportTypes.Contains(ExportType.Markdown))
            {
                exporters.Add(new MarkdownExporter(exportArgs.ShowHidden));
            }
            if (exportArgs.ExportTypes.Contains(ExportType.Html))
            {
                var headerProvider = new CompositeHeaderProvider(
                    [
                        new MetaHeaderProvider(),
                        new HighlightHeaderProvider(),
                        new MathjaxHeaderProvider(),
                        new GlightboxHeaderProvider(),
                    ]
                );

                var formatter = exportArgs.HtmlFormat == HtmlFormat.Bootstrap ? new BootstrapHtmlFormatter(headerProvider) as IHtmlFormatter : new TailwindHtmlFormatter(headerProvider);
                exporters.Add(new HtmlExporter(formatter, exportArgs.ShowHidden));
            }

            return exporters;
        }
    }
}
