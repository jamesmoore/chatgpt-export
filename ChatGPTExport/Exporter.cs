using System.IO.Abstractions;
using ChatGPTExport.Assets;
using ChatGPTExport.Exporters;
using ChatGPTExport.Models;

namespace ChatGPTExport
{
    public class Exporter(IFileSystem fileSystem, IEnumerable<IExporter> exporters, ExportMode exportMode)
    {
        public void Process(IEnumerable<(IAssetLocator AssetLocator, Conversation conversation)> conversations, IDirectoryInfo destination)
        {
            try
            {
                if (conversations.Select(p => p.conversation.conversation_id).Distinct().Count() > 1)
                {
                    throw new ApplicationException("Unable to export multiple conversations at once");
                }

                var fileContentsMap = new Dictionary<string, IEnumerable<string>>();

                var titles = string.Join(Environment.NewLine, conversations.Select(p => p.conversation.title).Distinct().ToArray());
                Console.WriteLine(titles);

                foreach (var (assetLocator, conversation) in conversations)
                {
                    Console.WriteLine($"\tMessages: {conversation.mapping.Count}\tLeaves: {conversation.mapping.Count(p => p.Value.IsLeaf())}");
                    foreach (var exporter in exporters)
                    {
                        Console.Write($"\t\t{exporter.GetType().Name}");

                        if (exportMode == ExportMode.Complete)
                        {
                            var completeFilename = GetFilename(conversation, "", exporter.GetExtension());
                            ExportConversation(fileContentsMap, assetLocator, exporter, conversation, completeFilename);
                        }
                        else if (exportMode == ExportMode.Latest)
                        {
                            var latest = conversation.GetLastestConversation();
                            var filename = GetFilename(latest, "", exporter.GetExtension());
                            ExportConversation(fileContentsMap, assetLocator, exporter, latest, filename);
                        }

                        Console.WriteLine($"...Done");
                    }
                }

                foreach (var kv in fileContentsMap)
                {
                    var destinationFilename = fileSystem.Path.Join(destination.FullName, kv.Key);
                    var contents = string.Join(Environment.NewLine, kv.Value);
                    var destinationExists = fileSystem.File.Exists(destinationFilename);
                    if (destinationExists == false || destinationExists && fileSystem.File.ReadAllText(destinationFilename) != contents)
                    {
                        fileSystem.File.WriteAllText(destinationFilename, contents);
                        var lastConversation = conversations.Last().conversation;
                        fileSystem.File.SetCreationTimeUtcIfPossible(destinationFilename, lastConversation.GetCreateTime().DateTime);
                        fileSystem.File.SetLastWriteTimeUtc(destinationFilename, lastConversation.GetUpdateTime().DateTime);
                        Console.WriteLine($"\t{kv.Key}...Saved");
                    }
                    else
                    {
                        Console.WriteLine($"\t{kv.Key}...No change");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        private static void ExportConversation(Dictionary<string, IEnumerable<string>> fileContentsMap, IAssetLocator assetLocator, IExporter exporter, Conversation conversation, string filename)
        {
            try
            {
                fileContentsMap[filename] = exporter.Export(assetLocator, conversation);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private string GetFilename(Conversation conversation, string modifier, string extension)
        {
            var createtime = conversation.GetCreateTime();
            var value = $"{createtime:yyyy-MM-ddTHH-mm-ss} - {conversation.title}{(string.IsNullOrWhiteSpace(modifier) ? "" : $" - {modifier}")}";
            value = new string(value.Where(p => fileSystem.Path.GetInvalidFileNameChars().Contains(p) == false).ToArray());
            return value + extension;
        }
    }
}
