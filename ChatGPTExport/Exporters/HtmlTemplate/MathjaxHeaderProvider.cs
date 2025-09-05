namespace ChatGPTExport.Exporters.HtmlTemplate
{
    internal class MathjaxHeaderProvider : IHeaderProvider
    {

        public string GetHeaders(HtmlPage htmlPage)
        {
            return htmlPage.HasMath ? """  <script id="MathJax-script" async src="https://cdn.jsdelivr.net/npm/mathjax@4/tex-mml-chtml.js"></script>""" : "";
        }
    }
}
