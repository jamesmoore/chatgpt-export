using Markdig;

namespace ChatGPTExport.Exporters.HtmlTemplate
{
    internal class TailwindHtmlFormatter(IHeaderProvider headerProvider) : IHtmlFormatter
    {
        public void ApplyMarkdownPipelineBuilder(MarkdownPipelineBuilder markdownPipelineBuilder)
        {
        }

        public string FormatHtmlPage(PageContent pageContent)
        {
            return $$"""
<!doctype html>
<html lang="en" class="dark">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>{{pageContent.Title}}</title>

  <!-- Tailwind (with Typography plugin) -->
  <script>
    window.tailwind = { config: { darkMode: 'class' } };
  </script>
  <script src="https://cdn.tailwindcss.com?plugins=typography"></script>

{{headerProvider.GetHeaders()}}
</head>
<body class="bg-neutral-900 text-neutral-100 antialiased">
<div class="container mx-auto max-w-4xl px-4 py-6">
<h1 class="text-3xl font-semibold mb-6">{{pageContent.Title}}</h1>
<div class="prose prose-invert leading-relaxed max-w-[80ch] md:max-w-[90ch]
            prose-p:my-2 prose-li:my-1
            prose-ul:list-disc prose-ol:list-decimal
            prose-pre:overflow-x-auto
            prose-hr:my-4
            marker:text-neutral-400">
  {{string.Join("", pageContent.Body)}}
</div>
</div> 
</body>
</html>
""";
        }

        public string FormatUserInput(string html)
        {
            return $"""
<div class="flex justify-end mb-2">
  <div class="ml-auto w-full sm:w-5/6 md:w-2/3 lg:w-2/3 rounded-xl bg-neutral-800 px-3 break-words">
      {html}
  </div>
</div>
""";
        }
    }
}
