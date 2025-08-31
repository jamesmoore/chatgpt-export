namespace ChatGPTExport.Exporters
{
    public class HtmlPage
    {
        public string Title {  get; set; }
        public IEnumerable<HtmlFragment> Body { get; set; }

        public string GetBodyString() => string.Join(Environment.NewLine, Body);
        public bool HasCode => Body.Any(p => p.HasCode);
    }

    public class HtmlFragment
    {
        public string Html { get; set; }
        public bool HasCode { get; set; }
        public override string ToString() => Html;
    }
}
