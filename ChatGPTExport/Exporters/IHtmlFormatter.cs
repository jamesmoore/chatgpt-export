
using ChatGPTExport.Exporters.HtmlTemplate;
using Markdig;

namespace ChatGPTExport.Exporters
{
    internal interface IHtmlFormatter
    {
        void ApplyMarkdownPipelineBuilder(MarkdownPipelineBuilder markdownPipelineBuilder);
        string FormatHtmlPage(PageContent pageContent);
        string FormatUserInput(string html);
    }
}