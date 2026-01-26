using Markdig;

namespace ChatGPTExport.Formatters.Html
{
    internal interface IHtmlFormatter
    {
        void ApplyMarkdownPipelineBuilder(MarkdownPipelineBuilder markdownPipelineBuilder);
        string FormatHtmlPage(HtmlPage page);
        string FormatUserInput(string html);
    }
}