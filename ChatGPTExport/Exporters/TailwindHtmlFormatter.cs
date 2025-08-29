namespace ChatGPTExport.Exporters
{
    internal class TailwindHtmlFormatter : IHtmlFormatter
    {
        public string FormatHtmlPage(string titleString, IEnumerable<string> bodyHtml)
        {
            return $$"""
<!doctype html>
<html lang="en" class="dark">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>{{titleString}}</title>

  <!-- Tailwind (with Typography plugin) -->
  <script>
    window.tailwind = { config: { darkMode: 'class' } };
  </script>
  <script src="https://cdn.tailwindcss.com?plugins=typography"></script>

  <!-- highlight.js (dark theme) -->
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.11.1/styles/github-dark.min.css">
  <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.11.1/highlight.min.js"></script>
  <script>hljs.highlightAll();</script>
</head>
<body class="bg-neutral-900 text-neutral-100 antialiased">
<div class="container mx-auto max-w-4xl px-4 py-6">
<h1 class="text-3xl font-semibold mb-6">{{titleString}}</h1>
<div class="prose prose-invert leading-relaxed max-w-[80ch] md:max-w-[90ch]
            prose-p:my-2 prose-li:my-1
            prose-ul:list-disc prose-ol:list-decimal
            prose-pre:overflow-x-auto
            prose-hr:my-4
            marker:text-neutral-400">
  {{string.Join("", bodyHtml)}}
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
  <div class="ml-auto w-full sm:w-5/6 md:w-2/3 lg:w-1/2 rounded-xl bg-neutral-800 px-3 break-words">
      {html}
  </div>
</div>
""";
        }
    }
}
