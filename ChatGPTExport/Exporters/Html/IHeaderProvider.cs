namespace ChatGPTExport.Exporters.Html
{
    internal interface IHeaderProvider
    {
        string GetHeaders(HtmlPage htmlPage);
    }
}