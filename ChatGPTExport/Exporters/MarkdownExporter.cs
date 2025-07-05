using System.IO.Abstractions;
using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    internal class MarkdownExporter(IFileSystem fileSystem, IDirectoryInfo sourceDirectory, IDirectoryInfo destinationDirectory) : IExporter
    {
        private readonly string LineBreak = Environment.NewLine;

        public void Export(Conversation conversation, string filename)
        {
            var messages = conversation.mapping.Select(p => p.Value).
                Where(p => p.message != null).
                Select(p => p.message).
                Where(p => p.content != null);

            var strings = new List<string>();

            var visitor = new ContentVisitor(fileSystem, sourceDirectory, destinationDirectory);

            foreach (var message in messages)
            {
                try
                {
                    var (itemContent, suffix) = message.Accept(visitor);

                    if (itemContent.Any())
                    {
                        var authorname = string.IsNullOrWhiteSpace(message.author.name) ? "" : $" ({message.author.name})";
                        strings.Add($"**{message.author.role}{authorname}{suffix}**:  "); // double space for line break
                        strings.AddRange(itemContent);
                        strings.Add(LineBreak);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }

            var outputPath = fileSystem.Path.Join(destinationDirectory.FullName, filename);
            fileSystem.File.WriteAllText(outputPath, string.Join(LineBreak, strings));

            fileSystem.File.SetCreationTimeUtcIfPossible(outputPath, conversation.GetCreateTime().DateTime);
            fileSystem.File.SetLastWriteTimeUtc(outputPath, conversation.GetUpdateTime().DateTime);
        }

        public string GetExtension() => ".md";
    }
}
