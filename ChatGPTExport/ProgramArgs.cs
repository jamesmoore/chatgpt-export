using System.IO;
using ChatGPTExport.Exporters.Html;

namespace ChatGPTExport
{
    internal class ProgramArgs
    {
        public DirectoryInfo[]? SourceDirectory { get; set; }
        public DirectoryInfo? DestinationDirectory { get; set; }
        public ExportMode ExportMode { get; set; }
        public bool Validate { get; set; }
        public bool Json { get; set; }
        public bool Markdown { get; set; }
        public bool Html { get; set; }
        public HtmlFormat HtmlFormat { get; set; }
        public bool ShowHidden { get; set; }
    }
}
