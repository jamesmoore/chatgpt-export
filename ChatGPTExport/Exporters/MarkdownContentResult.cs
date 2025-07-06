namespace ChatGPTExport.Exporters
{
    public record MarkdownContentResult(IEnumerable<string> Lines, string? Suffix = null)
    {
        public MarkdownContentResult(string line, string? Suffix = null) : this([line], Suffix) { }

        public MarkdownContentResult() : this([]) { }
    };
}
