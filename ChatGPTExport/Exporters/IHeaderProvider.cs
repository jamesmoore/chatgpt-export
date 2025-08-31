namespace ChatGPTExport.Exporters
{
    internal interface IHeaderProvider
    {
        string GetHeaders(HtmlPage htmlPage);
    }
}