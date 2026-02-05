using System.IO.Abstractions;
using ChatGPTExport;
using ChatGPTExport.Formatters.Html;

namespace ChatGpt.Exporter.Cli
{
    internal record ExportArgs(
        IEnumerable<IDirectoryInfo> SourceDirectory, 
        IDirectoryInfo DestinationDirectory, 
        ExportMode ExportMode, 
        IEnumerable<ExportType> ExportTypes, 
        HtmlFormat HtmlFormat, 
        bool ShowHidden);
}
