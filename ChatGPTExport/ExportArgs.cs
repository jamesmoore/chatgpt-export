using System.IO.Abstractions;
using ChatGPTExport.Exporters.Html;

namespace ChatGPTExport
{
    internal record ExportArgs(
        IEnumerable<IDirectoryInfo> SourceDirectory, 
        IDirectoryInfo DestinationDirectory, 
        ExportMode ExportMode, 
        bool Validate,
        IEnumerable<ExportType> ExportTypes, 
        HtmlFormat HtmlFormat, 
        bool ShowHidden);

    public enum ExportType
    {
        None = 0,
        Json = 1,
        Markdown = 2,
        Html = 4,
    }
}
