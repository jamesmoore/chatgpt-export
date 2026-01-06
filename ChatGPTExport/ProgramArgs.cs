using System.IO;
using ChatGPTExport.Exporters.Html;

namespace ChatGPTExport
{
    internal record ProgramArgs(
        DirectoryInfo[]? SourceDirectory, 
        DirectoryInfo? DestinationDirectory, 
        ExportMode ExportMode, 
        bool Validate, 
        bool Json, 
        bool Markdown, 
        bool Html, 
        HtmlFormat HtmlFormat, 
        bool ShowHidden);
}
