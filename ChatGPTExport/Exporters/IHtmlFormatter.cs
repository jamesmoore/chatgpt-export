
namespace ChatGPTExport.Exporters
{
    internal interface IHtmlFormatter
    {
        string FormatHtmlPage(string titleString, IEnumerable<string> bodyHtml);
        string FormatUserInput(string html);
    }
}