namespace ChatGPTExport.Exporters.Html
{
    public record HtmlPage(string Title, IEnumerable<HtmlFragment> Body)
    {
        public string GetBodyString() => string.Join(Environment.NewLine, Body);
        public bool HasCode => Body.Any(p => p.HasCode);
        public bool HasMath => Body.Any(p => p.HasMath);
        public bool HasImage => Body.Any(_ => _.HasImage);
        public IReadOnlyCollection<string> Languages => Body.SelectMany(p => p.Languages).Distinct().ToList();
    }

    public class HtmlFragment
    {
        public string Html { get; set; }
        public bool HasCode { get; set; }
        public bool HasMath { get; set; }
        public bool HasImage { get; set; }
        public IReadOnlyCollection<string> Languages { get; set; }
        public override string ToString() => Html;
    }
}
