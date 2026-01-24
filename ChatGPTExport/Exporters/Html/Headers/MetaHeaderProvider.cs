namespace ChatGPTExport.Exporters.Html.Headers
{
    internal class MetaHeaderProvider : IHeaderProvider
    {
        public string GetHeaders(HtmlPage htmlPage)
        {
            var headers = new List<string>();
            foreach (var meta in htmlPage.MetaHeaders)
            {
                headers.Add($"<meta name=\"{meta.Key}\" content=\"{meta.Value}\">");
            }
            return string.Join(Environment.NewLine, headers);
        }
    }
}
