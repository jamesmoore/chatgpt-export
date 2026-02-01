using ChatGPTExport.Exporters.Html;
using ChatGPTExport.Formatters;
using ChatGPTExport.Formatters.Html;
using ChatGPTExport.Formatters.Html.Headers;
using ChatGPTExport.Formatters.Html.Template;
using ChatGPTExport.Formatters.Json;
using ChatGPTExport.Formatters.Markdown;

namespace ChatGPTExport
{
    public class ConversationFormatterFactory
    {
        public IEnumerable<IConversationFormatter> GetFormatters(
            IEnumerable<ExportType> exportTypes,
            HtmlFormat htmlFormat,
            bool showHidden
            )
        {
            var exporters = new List<IConversationFormatter>();
            if (exportTypes.Contains(ExportType.Json))
            {
                exporters.Add(new JsonFormatter());
            }
            if (exportTypes.Contains(ExportType.Markdown))
            {
                exporters.Add(new MarkdownFormatter(showHidden));
            }
            if (exportTypes.Contains(ExportType.Html))
            {
                var headerProvider = new CompositeHeaderProvider(
                    [
                        new MetaHeaderProvider(),
                        new HighlightHeaderProvider(),
                        new MathjaxHeaderProvider(),
                        new GlightboxHeaderProvider(),
                    ]
                );

                var formatter = htmlFormat == HtmlFormat.Bootstrap ? new BootstrapHtmlFormatter(headerProvider) as IHtmlFormatter : new TailwindHtmlFormatter(headerProvider);
                exporters.Add(new HtmlFormatter(formatter, showHidden));
            }

            return exporters;
        }
    }
}
