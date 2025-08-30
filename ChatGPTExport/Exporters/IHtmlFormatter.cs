
using Markdig;

namespace ChatGPTExport.Exporters
{
    internal interface IHtmlFormatter
    {
        void ApplyMarkdownPipelineBuilder(MarkdownPipelineBuilder markdownPipelineBuilder);
        string FormatHtmlPage(string titleString, IEnumerable<string> bodyHtml);
        string FormatUserInput(string html);
    }
}