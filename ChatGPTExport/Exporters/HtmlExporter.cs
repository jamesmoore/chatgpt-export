using ChatGPTExport.Assets;
using ChatGPTExport.Models;
using Markdig;

namespace ChatGPTExport.Exporters
{
    internal class HtmlExporter(MarkdownExporter markdownExporter) : IExporter
    {
        public IEnumerable<string> Export(IAssetLocator assetLocator, Conversation conversation)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()     // tables, footnotes, lists, etc.
                .UsePipeTables()
                .UseBootstrap()              // optional: nicer HTML classes
                .Build();

            var markdown = markdownExporter.Export(assetLocator, conversation);

            var bodyHtml = markdown.Select(p => Markdown.ToHtml(p, pipeline));

            // Wrap in a minimal HTML document (you can add CSS/JS here)
            string html = $"""
<!doctype html>
<html lang="en" data-bs-theme="dark">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>{conversation.title}</title>
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/normalize/8.0.1/normalize.min.css">
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.7/css/bootstrap.min.css">
</head>
<body class="container">
{string.Join("", bodyHtml)}
</body>
</html>
""";

            return [html];
        }

        public string GetExtension() => ".html";
    }
}
