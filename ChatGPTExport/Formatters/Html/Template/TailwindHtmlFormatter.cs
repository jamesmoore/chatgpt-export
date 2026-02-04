using Markdig;

namespace ChatGPTExport.Formatters.Html.Template
{
    internal class TailwindHtmlFormatter(IHeaderProvider headerProvider) : IHtmlFormatter
    {
        public void ApplyMarkdownPipelineBuilder(MarkdownPipelineBuilder markdownPipelineBuilder)
        {
        }

        public string FormatHtmlPage(HtmlPage page)
        {
            return $$"""
<!doctype html>
<html lang="en" class="system">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>{{page.Title}}</title>

  <!-- Tailwind (with Typography plugin) -->
  <script>
    window.tailwind = { config: { darkMode: 'class' } };
  </script>
  <script src="https://cdn.tailwindcss.com?plugins=typography"></script>

  <style>
  /* Remove inline-code styling when code is inside a pre block */
  .prose pre code {
      background-color: transparent !important;
      padding: 0 !important;
      border-radius: 0 !important;
      color: inherit !important;
  }
  </style>

<style>
:root {
  --scrollbar-thumb: rgb(163 163 163); /* neutral-400 */
}

@media (prefers-color-scheme: dark) {
  :root {
    --scrollbar-thumb: rgb(82 82 82); /* neutral-600 */
  }
}

/* WebKit */
::-webkit-scrollbar-track {
  background: transparent;
}

::-webkit-scrollbar-thumb {
  background-color: var(--scrollbar-thumb);
}

/* Firefox */
* {
  scrollbar-color: var(--scrollbar-thumb) transparent;
}
</style>

{{headerProvider.GetHeaders(page)}}
</head>
<body class="dark:bg-neutral-900 dark:text-neutral-100 antialiased">
<div class="container mx-auto max-w-4xl px-4 py-6">
<h1 class="text-3xl font-semibold mb-6">{{page.Title}}</h1>
<div class="prose dark:prose-invert leading-relaxed max-w-[80ch] md:max-w-[90ch]
            prose-p:my-2 prose-li:my-1
            prose-ul:list-disc prose-ol:list-decimal
            prose-pre:overflow-x-auto
            prose-hr:my-4
            marker:text-neutral-400

            prose-code:before:content-none
            prose-code:after:content-none
            prose-code:bg-neutral-300
            dark:prose-code:bg-neutral-700
            prose-code:rounded
            prose-code:px-[0.3rem]
            prose-code:py-[0.15rem]
            prose-code:text-[--tw-prose-code]
            ">
  {{page.GetBodyString()}}
</div>
</div> 
</body>
</html>
""";
        }

        public string FormatUserInput(string html)
        {
            return $"""
<div class="flex justify-end my-4">
  <div class="ml-auto w-full sm:w-5/6 md:w-2/3 lg:w-2/3 rounded-xl bg-neutral-100 dark:bg-neutral-800 px-3 break-words">
      {html}
  </div>
</div>
""";
        }
    }
}
