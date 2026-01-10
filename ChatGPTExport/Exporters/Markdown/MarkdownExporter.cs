using ChatGPTExport.Assets;
using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters.Markdown
{
    internal class MarkdownExporter(bool showHidden) : IExporter
    {
        private readonly string LineBreak = Environment.NewLine;

        public IEnumerable<string> Export(IAssetLocator assetLocator, Conversation conversation)
        {
            var messages = conversation.GetMessagesWithContent();

            var strings = new List<string>();

            var visitor = new MarkdownContentVisitor(assetLocator, showHidden);

            foreach (var message in messages)
            {
                try
                {
                    var visitResult = message.Accept(visitor);

                    if (message.author != null && visitResult != null && visitResult.Lines.Any())
                    {
                        var authorname = string.IsNullOrWhiteSpace(message.author.name) ? "" : $" ({message.author.name})";
                        strings.Add($"**{message.author.role}{authorname}{visitResult.Suffix}**:  "); // double space for line break
                        strings.Add(string.Join(LineBreak, visitResult.Lines));
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
