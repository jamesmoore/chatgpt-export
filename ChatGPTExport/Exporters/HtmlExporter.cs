using System.Net;
using ChatGPTExport.Assets;
using ChatGPTExport.Models;
using Markdig;

namespace ChatGPTExport.Exporters
{
    internal class HtmlExporter() : IExporter
    {
        private readonly string LineBreak = Environment.NewLine;

        public IEnumerable<string> Export(IAssetLocator assetLocator, Conversation conversation)
        {
            var messages = conversation.GetMessagesWithContent();

            var strings = new List<(Author Author, string Content)>();

            var visitor = new MarkdownContentVisitor(assetLocator);

            foreach (var message in messages)
            {
                try
                {
                    var (messageContent, suffix) = message.Accept(visitor);

                    if (messageContent.Any())
                    {
                        strings.Add((message.author, string.Join(LineBreak, messageContent)));
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }

            var markdownPipeline = GetPipeline();
            var bodyHtml = strings.Select(p => GetHtmlChunks(p.Author, p.Content, markdownPipeline));

            var titleString = WebUtility.HtmlEncode(conversation.title);
            var html = $$"""
<!doctype html>
<html lang="en" data-bs-theme="dark">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>{{titleString}}</title>
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.7/css/bootstrap.min.css">
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.11.1/styles/github-dark.min.css">
  <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.11.1/highlight.min.js"></script>
  <script>hljs.highlightAll();</script>
  <style>
    .user-container {
      display: flex;
      justify-content: flex-end;
      margin-bottom: 10px;
    }

    .user {
      background-color: var(--bs-secondary-bg);
      padding: 10px 10px 0px 10px;
      border-radius: 10px;
      max-width: 66%;
    }

    .user p {
      margin-bottom: 10px;
    }
  </style>
</head>
<body class="container">
<div class="my-4">
  <h1>{{titleString}}</h1>
</div>
{{string.Join("", bodyHtml)}}
</body>
</html>
""";

            return [html];
        }

        private static string GetHtmlChunks(Author author, string content, MarkdownPipeline markdownPipeline)
        {
            var html = Markdown.ToHtml(content, markdownPipeline);

            if(author.role == "user")
            {
                return $"""<div class="user-container"><div class="user">{html}</div></div>""";
            }
            else
            {
                return html;
            }
        }

        private static MarkdownPipeline GetPipeline()
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()     // tables, footnotes, lists, etc.
                .UsePipeTables()
                .UseBootstrap()              // optional: nicer HTML classes
                .Build();
            return pipeline;
        }

        public string GetExtension() => ".html";
    }
}
