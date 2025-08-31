using Markdig;

namespace ChatGPTExport.Exporters
{
    internal interface IHtmlFormatter
    {
        void ApplyMarkdownPipelineBuilder(MarkdownPipelineBuilder markdownPipelineBuilder);
        string FormatHtmlPage(HtmlPage page);
        string FormatUserInput(string html);
    }
}