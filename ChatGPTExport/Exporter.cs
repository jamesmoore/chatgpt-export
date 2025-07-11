using System.IO.Abstractions;
using ChatGPTExport.Exporters;
using ChatGPTExport.Models;

namespace ChatGPTExport
{
    public class Exporter(IFileSystem fileSystem)
    {
        public void Process(IEnumerable<(IFileInfo source, Conversation conversation)> conversations, IDirectoryInfo destination)
        {
            if (conversations.Select(p => p.conversation.conversation_id).Distinct().Count() > 1)
            {
                throw new ApplicationException("Unable to export multiple conversations at once");
            }

            var fileContentsMap = new Dictionary<string, IEnumerable<string>>();

            foreach (var (source, conversation) in conversations)
            {
                var exporters = new List<IExporter>()
                    {
                        new JsonExporter(fileSystem, destination),
                        new MarkdownExporter(fileSystem, source.Directory, destination),
                    };

                Console.WriteLine($"{conversation.title}\tMessages: {conversation.mapping.Count}\tLeaves: {conversation.mapping.Count(p => p.Value.IsLeaf())}");
                foreach (var exporter in exporters)
                {
                    Console.WriteLine($"\t{exporter.GetType().Name}");

                    if (conversation.HasMultipleBranches())
                    {
                        var completeFilename = GetFilename(conversation, "complete", exporter.GetExtension());
                        fileContentsMap[completeFilename] = exporter.Export(conversation);
                    }

                    var latest = conversation.GetLastestConversation();
                    var filename = GetFilename(latest, "", exporter.GetExtension());
                    fileContentsMap[filename] = exporter.Export(latest);
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
                    fileSystem.File.SetCreationTimeUtcIfPossible(destinationFilename, conversations.Last().conversation.GetCreateTime().DateTime);
                    fileSystem.File.SetLastWriteTimeUtc(destinationFilename, conversations.Last().conversation.GetUpdateTime().DateTime);
                }
            }
        }

        private string GetFilename(Conversation conversation, string modifier, string extension)
        {
            var createtime = conversation.GetCreateTime();
            var value = createtime.ToString("yyyy-MM-ddTHH-mm-ss") + " - " + conversation.title + (string.IsNullOrWhiteSpace(modifier) ? "" : " - " + modifier);
            value = new string(value.Where(p => fileSystem.Path.GetInvalidFileNameChars().Contains(p) == false).ToArray());
            return value + extension;
        }
    }
}
