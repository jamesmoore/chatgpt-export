using System.IO.Abstractions;
using ChatGPTExport.Exporters;
using ChatGPTExport.Models;

namespace ChatGPTExport
{
    public class Exporter(IFileSystem fileSystem)
    {
        public void Process(IFileInfo source, Conversations conversations, IDirectoryInfo destination)
        {
            if (source.Exists)
            {
                var exporters = new List<IExporter>()
                {
                    new JsonExporter(fileSystem, destination),
                    new MarkdownExporter(fileSystem, source.Directory, destination),
                };

                foreach (var conversation in conversations)
                {
                    Console.WriteLine(conversation.title + "\tMessages: " + conversation.mapping.Count + "\tLeaves: " + conversation.mapping.Count(p => p.Value.IsLeaf()));

                    if (conversation.HasMultipleBranches())
                    {
                        foreach (var exporter in exporters)
                        {
                            var filename = GetFilename(conversation, "complete", exporter.GetExtension());
                            exporter.Export(conversation, filename);
                        }
                    }

                    var latest = conversation.GetLastestConversation();
                    foreach (var exporter in exporters)
                    {
                        var filename = GetFilename(latest, "", exporter.GetExtension());
                        exporter.Export(latest, filename);
                    }
                }
            }
            else
            {
                throw new ApplicationException($"{source.FullName} not found");
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
