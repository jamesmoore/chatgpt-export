namespace ChatGPTExport.Formatters.Html
{
    internal interface IHeaderProvider
    {
        string GetHeaders(HtmlPage htmlPage);
    }
}