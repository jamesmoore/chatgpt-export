using ChatGPTExport.Assets;
using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    internal class MarkdownExporter : IExporter
    {
        private readonly string LineBreak = Environment.NewLine;

        public IEnumerable<string> Export(IAssetLocator assetLocator, Conversation conversation)
        {
            var messages = conversation.mapping.Select(p => p.Value).
                Where(p => p.message != null).
                Select(p => p.message).
                Where(p => p.content != null);

            var strings = new List<string>();

            var visitor = new MarkdownContentVisitor(assetLocator);

            foreach (var message in messages)
            {
                try
                {
                    var (messageContent, suffix) = message.Accept(visitor);

                    if (messageContent.Any())
                    {
                        var authorname = string.IsNullOrWhiteSpace(message.author.name) ? "" : $" ({message.author.name})";
                        strings.Add($"**{message.author.role}{authorname}{suffix}**:  "); // double space for line break
                        strings.Add(string.Join(LineBreak, messageContent));
                        strings.Add(LineBreak);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }

            return strings;
        }

        public string GetExtension() => ".md";
    }
}
