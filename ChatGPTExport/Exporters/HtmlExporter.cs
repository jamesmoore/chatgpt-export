using System.Net;
using ChatGPTExport.Assets;
using ChatGPTExport.Models;
using Markdig;

namespace ChatGPTExport.Exporters
{
    internal class HtmlExporter(IHtmlFormatter formatter) : IExporter
    {
        private readonly string LineBreak = Environment.NewLine;

        public IEnumerable<string> Export(IAssetLocator assetLocator, Conversation conversation)
        {
            var messages = conversation.GetMessagesWithContent();

            var strings = new List<(Author Author, string Content)>();

            var visitor = new MarkdownContentVisitor(assetLocator);

            foreach (var message in messages)
            {
                try
                {
                    var (messageContent, suffix) = message.Accept(visitor);

                    if (messageContent.Any())
                    {
                        strings.Add((message.author, string.Join(LineBreak, messageContent)));
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }

            var markdownPipeline = GetPipeline();
            var bodyHtml = strings.Select(p => GetHtmlChunks(p.Author, p.Content, markdownPipeline));

            var titleString = WebUtility.HtmlEncode(conversation.title);
            string html = formatter.FormatHtmlPage(titleString, bodyHtml);

            return [html];
        }

        private string GetHtmlChunks(Author author, string content, MarkdownPipeline markdownPipeline)
        {
            var html = Markdown.ToHtml(content, markdownPipeline);

            if(author.role == "user")
            {
                return formatter.FormatUserInput(html);
            }
            else
            {
                return html;
            }
        }

        private MarkdownPipeline GetPipeline()
        {
            var pipelineBuilder = new MarkdownPipelineBuilder()
                //.UseAlertBlocks()
                //.UseAbbreviations()
                .UseAutoIdentifiers()
                //.UseCitations()
                //.UseCustomContainers()
                //.UseDefinitionLists()
                //.UseEmphasisExtras()
                //.UseFigures()
                //.UseFooters()
                .UseFootnotes()
                //.UseGridTables()
                //.UseMathematics()
                //.UseMediaLinks()
                .UsePipeTables()
                .UseListExtras()
                .UseTaskLists()
                //.UseDiagrams()
                .UseAutoLinks();
            //.UseGenericAttributes(); 

            formatter.ApplyMarkdownPipelineBuilder(pipelineBuilder);

            var pipeline = pipelineBuilder.Build();
            return pipeline;
        }

        public string GetExtension() => ".html";
    }
}
