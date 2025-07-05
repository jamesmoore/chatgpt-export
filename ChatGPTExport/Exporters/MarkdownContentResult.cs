namespace ChatGPTExport.Exporters
{
    public record MarkdownContentResult(IEnumerable<string> Lines, string? Suffix = null);
}
