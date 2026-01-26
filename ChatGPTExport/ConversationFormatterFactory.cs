using ChatGPTExport.Exporters.Html;
using ChatGPTExport.Formatters;
using ChatGPTExport.Formatters.Html;
using ChatGPTExport.Formatters.Html.Headers;
using ChatGPTExport.Formatters.Html.Template;
using ChatGPTExport.Formatters.Json;
using ChatGPTExport.Formatters.Markdown;

namespace ChatGPTExport
{
    internal class ConversationFormatterFactory
    {
        public IEnumerable<IConversationFormatter> GetFormatters(ExportArgs exportArgs)
        {
            var exporters = new List<IConversationFormatter>();
            if (exportArgs.ExportTypes.Contains(ExportType.Json))
            {
                exporters.Add(new JsonFormatter());
            }
            if (exportArgs.ExportTypes.Contains(ExportType.Markdown))
            {
                exporters.Add(new MarkdownFormatter(exportArgs.ShowHidden));
            }
            if (exportArgs.ExportTypes.Contains(ExportType.Html))
            {
                var headerProvider = new CompositeHeaderProvider(
                    [
                        new MetaHeaderProvider(),
                        new HighlightHeaderProvider(),
                        new MathjaxHeaderProvider(),
                        new GlightboxHeaderProvider(),
                    ]
                );

                var formatter = exportArgs.HtmlFormat == HtmlFormat.Bootstrap ? new BootstrapHtmlFormatter(headerProvider) as IHtmlFormatter : new TailwindHtmlFormatter(headerProvider);
                exporters.Add(new HtmlFormatter(formatter, exportArgs.ShowHidden));
            }

            return exporters;
        }
    }
}
