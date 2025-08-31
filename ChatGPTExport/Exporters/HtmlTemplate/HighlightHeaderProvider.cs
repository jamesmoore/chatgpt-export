namespace ChatGPTExport.Exporters.HtmlTemplate
{
    internal class HighlightHeaderProvider : IHeaderProvider
    {

        public string GetHeaders(HtmlPage htmlPage)
        {
            return htmlPage.HasCode ? """
  <!-- highlight.js (dark theme) -->
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.11.1/styles/github-dark.min.css">
  <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.11.1/highlight.min.js"></script>
  <script>hljs.highlightAll();</script>
""" : "";
        }
    }
}
