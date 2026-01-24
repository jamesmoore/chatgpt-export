namespace ChatGPTExport.Exporters.Html
{
    public record HtmlPage(string Title, IEnumerable<HtmlFragment> Body, Dictionary<string, string> MetaHeaders)
    {
        public string GetBodyString() => string.Join(Environment.NewLine, Body);
        public bool HasCode => Body.Any(p => p.HasCode);
        public bool HasMath => Body.Any(p => p.HasMath);
        public bool HasImage => Body.Any(_ => _.HasImage);
        public IReadOnlyCollection<string> Languages => Body.SelectMany(p => p.Languages).Distinct().ToList();
    }

    public record HtmlFragment(
        string Html,
        bool HasCode,
        bool HasMath,
        bool HasImage,
        IReadOnlyCollection<string> Languages)
    {
        public override string ToString() => Html;
    }
}
