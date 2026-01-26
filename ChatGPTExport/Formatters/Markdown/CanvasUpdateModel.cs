namespace ChatGPTExport.Formatters.Markdown
{
    public class CanvasUpdateModel
    {
        public Update[]? updates { get; set; }

        public class Update
        {
            public string? pattern { get; set; }
            public string? replacement { get; set; }
        }
    }
}
