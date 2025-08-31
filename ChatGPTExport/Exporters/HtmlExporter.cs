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
            var bodyHtml = strings.Select(p => GetHtmlChunks(p.Author, p.Content, markdownPipeline)).ToList();

            var titleString = WebUtility.HtmlEncode(conversation.title);
            string html = formatter.FormatHtmlPage(titleString, bodyHtml.Select(p => p.HtmlContent), bodyHtml.Any(p => p.PotentialMath));

            return [html];
        }

        private (string HtmlContent, bool PotentialMath) GetHtmlChunks(Author author, string markdown, MarkdownPipeline markdownPipeline)
        {
            var originalLength = markdown.Length;
            markdown = markdown.Replace(@"\[", "MARKDIG_MATHJAX_LEFT_BRACKET");
            markdown = markdown.Replace(@"\]", "MARKDIG_MATHJAX_RIGHT_BRACKET");
            markdown = markdown.Replace(@"\(", "MARKDIG_MATHJAX_LEFT_PARENTHESIS");
            markdown = markdown.Replace(@"\)", "MARKDIG_MATHJAX_RIGHT_PARENTHESIS");

            var potentialMath = markdown.Length != originalLength;

            var html = Markdown.ToHtml(markdown, markdownPipeline);

            html = html.Replace("MARKDIG_MATHJAX_LEFT_BRACKET", @"\[");
            html = html.Replace("MARKDIG_MATHJAX_RIGHT_BRACKET", @"\]");
            html = html.Replace("MARKDIG_MATHJAX_LEFT_PARENTHESIS", @"\(");
            html = html.Replace("MARKDIG_MATHJAX_RIGHT_PARENTHESIS", @"\)");

            if (author.role == "user")
            {
                return (formatter.FormatUserInput(html), potentialMath);
            }
            else
            {
                return (html, potentialMath);
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
