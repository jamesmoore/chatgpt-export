using Markdig;

namespace ChatGPTExport.Exporters.HtmlTemplate
{
    internal class BootstrapHtmlFormatter(IHeaderProvider headerProvider) : IHtmlFormatter
    {
        public void ApplyMarkdownPipelineBuilder(MarkdownPipelineBuilder markdownPipelineBuilder)
        {
            markdownPipelineBuilder.UseBootstrap();
        }

        public string FormatHtmlPage(PageContent pageContent)
        {
            return $$"""
<!doctype html>
<html lang="en" data-bs-theme="dark">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>{{pageContent.Title}}</title>
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.7/css/bootstrap.min.css">
{{headerProvider.GetHeaders()}}
</head>
<body class="container">
<div class="my-4">
  <h1>{{pageContent.Title}}</h1>
</div>
{{string.Join("", pageContent.Body)}}
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
