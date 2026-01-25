using ChatGPTExport.Assets;
using ChatGPTExport.Exporters;
using ChatGPTExport.Exporters.Html;
using ChatGPTExport.Exporters.Html.Headers;
using ChatGPTExport.Exporters.Html.Template;
using ChatGPTExport.Exporters.Json;
using ChatGPTExport.Exporters.Markdown;
using ChatGPTExport.Models;
using ChatGPTExport.Validators;
using System.IO;
using System.IO.Abstractions;

namespace ChatGPTExport
{

    internal class ExportBootstrap(IFileSystem fileSystem)
    {
        const string searchPattern = "conversations.json";

        public int RunExport(ExportArgs programArgs)
        {
            var destination = programArgs.DestinationDirectory;
            var sources = programArgs.SourceDirectory;
            var conversationsFactory = new ConversationsParser(fileSystem, programArgs.Validate);
            var exporters = GetExporters(programArgs);

            var exporter = new Exporter(fileSystem, exporters, programArgs.ExportMode);

            bool validationFail = false;

            Conversations? GetConversations(IFileInfo p)
            {
                try
                {
                    Console.WriteLine($"Loading conversation " + p.FullName);
                    return conversationsFactory.GetConversations(p);
                }
                catch (ValidationException)
                {
                    validationFail = true;
                    return null;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error parsing file: {p.FullName}{Environment.NewLine}\t{ex.Message}");
                    return null;
                }
            }

            var existingAssetLocator = new ExistingAssetLocator(fileSystem, destination);
            var conversationFiles = sources.Select(sourceDir => sourceDir.GetFiles(searchPattern, SearchOption.AllDirectories)).
                SelectMany(fileInfo => fileInfo, (fileInfo, file) => new { File = file, ParentDirecory = file.Directory }).ToList();

            var directoryConversationsMap = conversationFiles.Where(p => p.ParentDirecory != null)
                .Select(file => new
                {
                    AssetLocator = new AssetLocator(fileSystem, file.ParentDirecory!, destination, existingAssetLocator) as IAssetLocator,
                    Conversations = GetConversations(file.File)
                })
                .Where(x => x.Conversations != null)
                .OrderBy(x => x.Conversations!.GetUpdateTime())
                .ToList();

            if (validationFail)
            {
                throw new ApplicationException("Validation errors found");
            }

            var groupedByConversationId = directoryConversationsMap.Where(p => p.Conversations != null)
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

        static IEnumerable<IExporter> GetExporters(ExportArgs programArgs)
        {
            var exporters = new List<IExporter>();
            if (programArgs.ExportTypes.Contains(ExportType.Json))
            {
                exporters.Add(new JsonExporter());
            }
            if (programArgs.ExportTypes.Contains(ExportType.Markdown))
            {
                exporters.Add(new MarkdownExporter(programArgs.ShowHidden));
            }
            if (programArgs.ExportTypes.Contains(ExportType.Html))
            {
                var headerProvider = new CompositeHeaderProvider(
                    [
                        new MetaHeaderProvider(),
                        new HighlightHeaderProvider(),
                        new MathjaxHeaderProvider(),
                        new GlightboxHeaderProvider(),
                    ]
                );

                var formatter = programArgs.HtmlFormat == HtmlFormat.Bootstrap ? new BootstrapHtmlFormatter(headerProvider) as IHtmlFormatter : new TailwindHtmlFormatter(headerProvider);
                exporters.Add(new HtmlExporter(formatter, programArgs.ShowHidden));
            }

            return exporters;
        }
    }
}
