namespace ChatGPTExport.Assets
{
    public record Asset(string Name, string RelativePath)
    {
        public string GetMarkdownLink() => $"![{Name}]({RelativePath})  ";
    }
}