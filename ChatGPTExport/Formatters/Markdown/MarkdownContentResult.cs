namespace ChatGPTExport.Formatters.Markdown
{
    public record MarkdownContentResult(IEnumerable<string> Lines, string? Suffix = null, bool HasImage = false)
    {
        public MarkdownContentResult(string line, string? Suffix = null) : this([line], Suffix) { }

        public MarkdownContentResult() : this([]) { }
    };
}
