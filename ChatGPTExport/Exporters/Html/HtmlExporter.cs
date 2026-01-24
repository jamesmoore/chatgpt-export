using System.Net;
using System.Text.RegularExpressions;
using ChatGPTExport.Assets;
using ChatGPTExport.Models;
using Markdig;

namespace ChatGPTExport.Exporters.Html
{
    internal partial class HtmlExporter(IHtmlFormatter formatter, bool showHidden) : IExporter
    {
        private readonly string LineBreak = Environment.NewLine;

        public IEnumerable<string> Export(IAssetLocator assetLocator, Conversation conversation)
        {
            var messages = conversation.GetMessagesWithContent();

            var strings = new List<(Author Author, string Content, bool HasImage)>();

            var visitor = new MarkdownContentVisitor(assetLocator, showHidden);

            foreach (var message in messages)
            {
                try
                {
                    var visitResult = message.Accept(visitor);

                    if (message.author != null && visitResult != null && visitResult.Lines.Any())
                    {
                        strings.Add((message.author, string.Join(LineBreak, visitResult.Lines), visitResult.HasImage));
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }

            var markdownPipeline = GetPipeline();
            var bodyHtml = strings.Select(p => GetHtmlFragment(p.Author, p.Content, p.HasImage, markdownPipeline));

            var titleString = WebUtility.HtmlEncode(conversation.title ?? "No title");

            var metaHeaders = new Dictionary<string, string>();
            metaHeaders.Add("title", titleString);
            if(conversation.id != null)
                metaHeaders.Add("chatgpt_conversation_id", conversation.id);
            if(conversation.gizmo_id != null)
            metaHeaders.Add("chatgpt_gizmo_id", conversation.gizmo_id);
            metaHeaders.Add("chatgpt_created", conversation.GetCreateTime().ToString("s"));
            metaHeaders.Add("chatgpt_updated", conversation.GetUpdateTime().ToString("s"));

            string html = formatter.FormatHtmlPage(
                new HtmlPage(titleString, bodyHtml, metaHeaders));

            return [html];
        }

        [GeneratedRegex("```(.*)")]
        private static partial Regex MarkdownCodeBlockRegex();
        private static (bool HasCode, List<string> Languages) GetLanguages(string markdown)
        {
            var codeBlockRegex = MarkdownCodeBlockRegex().Matches(markdown);
            var languages = codeBlockRegex.Where(p => p.Groups.Count > 1).
                Select(p => p.Groups[1].Value).
                Where(p => string.IsNullOrWhiteSpace(p) == false).
                Select(p => p.Trim()).
                Distinct(StringComparer.OrdinalIgnoreCase).
                Select(v => v.ToLowerInvariant()).
                ToList();
            return (codeBlockRegex.Count > 0, languages);
        }

        private HtmlFragment GetHtmlFragment(Author author, string markdown, bool hasImage, MarkdownPipeline markdownPipeline)
        {
            var doc = Markdig.Markdown.Parse(markdown, markdownPipeline);

            var hasMath = false;

            if (markdown.Contains(@"\(") && markdown.Contains(@"\)") ||
                markdown.Contains(@"\[") && markdown.Contains(@"\]"))
            {
                var escaped = MarkdownMathConverter.ConvertBackslashMathToDollar(markdown);
                hasMath = markdown != escaped;
                markdown = escaped;
            }

            var html = Markdig.Markdown.ToHtml(markdown, markdownPipeline);

            var (HasCode, Languages) = GetLanguages(markdown);

            var fragment = new HtmlFragment(
                author.role == "user" ? formatter.FormatUserInput(html) : html,
                HasCode,
                hasMath,
                hasImage,
                Languages);
            return fragment;
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
                .UseMathematics()
                .UseMediaLinks()
                .UsePipeTables()
                .UseListExtras()
                .UseTaskLists()
                //.UseDiagrams()
                .UseAutoLinks()
                .DisableHtml();
            //.UseGenericAttributes(); 

            formatter.ApplyMarkdownPipelineBuilder(pipelineBuilder);

            var pipeline = pipelineBuilder.Build();
            return pipeline;
        }

        public string GetExtension() => ".html";
    }
}
