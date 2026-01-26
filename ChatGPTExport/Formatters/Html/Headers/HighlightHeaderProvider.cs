namespace ChatGPTExport.Formatters.Html.Headers
{
    internal class HighlightHeaderProvider : IHeaderProvider
    {
        const string version = "11.11.1";
        private static readonly string[] additionalLanguages = [
            "dockerfile",
            "powershell",
            "http",
            "latex",
            "ini",
        ];

        public string GetHeaders(HtmlPage htmlPage)
        {
            return htmlPage.HasCode ? GetCodeBlock(htmlPage.Languages) : "";
        }

        private static string GetCodeBlock(IEnumerable<string> languages)
        {
            var additionalScripts = languages.Intersect(additionalLanguages).Select(p => 
                $"""  <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/{version}/languages/{p}.min.js"></script>"""
                );

            var additionalStriptsBlock = string.Join(Environment.NewLine, additionalScripts);
            if (additionalStriptsBlock.Length > 0) {
                additionalStriptsBlock += Environment.NewLine;
            }

            return $"""
  <!-- highlight.js (dark theme) -->
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/{version}/styles/github-dark.min.css">
  <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/{version}/highlight.min.js"></script>
{additionalStriptsBlock}  <script>hljs.highlightAll();</script>
""";
        }
    }
}
