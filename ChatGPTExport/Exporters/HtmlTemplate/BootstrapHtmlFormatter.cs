using Markdig;

namespace ChatGPTExport.Exporters.HtmlTemplate
{
    internal class BootstrapHtmlFormatter : IHtmlFormatter
    {
        public void ApplyMarkdownPipelineBuilder(MarkdownPipelineBuilder markdownPipelineBuilder)
        {
            markdownPipelineBuilder.UseBootstrap();
        }

        public string FormatHtmlPage(string titleString, IEnumerable<string> bodyHtml)
        {
            return $$"""
<!doctype html>
<html lang="en" data-bs-theme="dark">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>{{titleString}}</title>
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.7/css/bootstrap.min.css">
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.11.1/styles/github-dark.min.css">
  <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.11.1/highlight.min.js"></script>
  <script>hljs.highlightAll();</script>

</head>
<body class="container">
<div class="my-4">
  <h1>{{titleString}}</h1>
</div>
{{string.Join("", bodyHtml)}}
</body>
</html>
""";
        }

        public string FormatUserInput(string html)
        {
            return $"""
<div class="d-flex justify-content-end mb-2">
    <div class="bg-body-secondary rounded-3 px-3 pt-3 ms-auto col-12 col-sm-10 col-md-8 col-lg-6 text-break">
    {html}
    </div>
</div>
""";
        }
    }
}
