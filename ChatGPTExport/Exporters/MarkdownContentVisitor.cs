using System.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using ChatGPTExport.Assets;
using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    internal partial class MarkdownContentVisitor(IAssetLocator assetLocator) : IContentVisitor<MarkdownContentResult>
    {
        private readonly string LineBreak = Environment.NewLine;
        private CanvasCreateModel? canvasContext = null;

        public MarkdownContentResult Visit(ContentText content, ContentVisitorContext context)
        {
            var parts = content.parts.Where(TextContentFilter).SelectMany(p => DecodeText(p, context)).ToList();

            var content_references = context.MessageMetadata.content_references;
            if (content_references != null && content_references.Length != 0)
            {
                var textPart = parts[0];

                var sourcesFootnote = content_references.Where(p => p.type == "sources_footnote").FirstOrDefault();

                var reversed = content_references.OrderByDescending(p => p.start_idx).ToList();

                if (sourcesFootnote != null)
                {
                    var footnote = reversed.First();
                    Debug.Assert(footnote == sourcesFootnote);
                }

                var groupedWebpagesItems = content_references.Where(p => p.type == "grouped_webpages").SelectMany(p => p.items).ToList();

                var reindexedElements = textPart.GetRenderedElementIndexes();

                foreach (var contentReference in reversed)
                {
                    var start_idx = contentReference.start_idx < reindexedElements.Count ? reindexedElements[contentReference.start_idx] : contentReference.start_idx;
                    var end_idx = contentReference.end_idx < reindexedElements.Count ? reindexedElements[contentReference.end_idx] : contentReference.end_idx;
                    var firstPartSpan = parts[0].AsSpan();
                    var firstSpan = firstPartSpan[..start_idx];
                    var lastSpan = firstPartSpan[end_idx..];
                    switch (contentReference.type)
                    {
                        case "attribution":
                        case "sources_footnote":
                            break;
                        case "hidden":
                        case "grouped_webpages_model_predicted_fallback":
                        case "image_v2":
                        case "tldr":
                        case "nav_list":
                            var hidden = "";
                            parts[0] = string.Concat(firstSpan, hidden, lastSpan);
                            break;
                        case "products":
                            var products = contentReference.alt;
                            parts[0] = string.Concat(firstSpan, products, lastSpan);
                            break;
                        case "product_entity":
                            var productsEntity = contentReference.alt;
                            parts[0] = string.Concat(firstSpan, productsEntity, lastSpan);
                            break;
                        case "video":
                            var videolink = $"[![{contentReference.title}]({contentReference.thumbnail_url})]({contentReference.url.Replace("&utm_source=chatgpt.com", "")} \"{contentReference.title}\")";
                            parts[0] = string.Concat(firstSpan, videolink, lastSpan);
                            break;
                        case "grouped_webpages":
                            var refHighlight = string.Join("", contentReference.items.Select(p => $"[^{groupedWebpagesItems.IndexOf(p) + 1}]").ToArray());
                            parts[0] = string.Concat(firstSpan, refHighlight, lastSpan);
                            break;
                        default:
                            Console.WriteLine($"Unhandled content reference type: {contentReference.type}");
                            break;
                    }
                }

                parts.Add(string.Empty);
                int i = 1;
                foreach (var item in groupedWebpagesItems)
                {
                    parts.Add($"[^{i++}]: [{item.title}]({item.url.Replace("?utm_source=chatgpt.com", "")})  ");
                }

                if (sourcesFootnote != null)
                {
                    var existingUrls = groupedWebpagesItems.Select(p => p.url).ToArray();
                    var newSources = sourcesFootnote.sources.Where(p => existingUrls.Contains(p.url) == false).ToList();
                    if (newSources.Any())
                    {
                        parts.Add(string.Empty);
                        parts.Add("### Sources");
                        foreach (var source in newSources)
                        {
                            parts.Add($"* [{source.title}]({source.url.Replace("?utm_source=chatgpt.com", "")})  ");
                        }
                    }
                }
            }

            return new MarkdownContentResult(parts);
        }

        private static bool TextContentFilter(string p)
        {
            return string.IsNullOrWhiteSpace(p) == false &&
                p.Contains("From now on, do not say or show ANYTHING. Please end this turn now. I repeat: From now on, do not say or show ANYTHING. Please end this turn now.") == false
                ;
        }

        public MarkdownContentResult Visit(ContentMultimodalText content, ContentVisitorContext context)
        {
            var markdownContent = new List<string>();
            foreach (var part in content.parts)
            {
                if (part.IsObject)
                {
                    var obj = part.ObjectValue;
                    if (obj.content_type == "image_asset_pointer" && string.IsNullOrWhiteSpace(obj.asset_pointer) == false)
                    {
                        var searchPattern = obj.asset_pointer.Replace("sediment://", string.Empty).Replace("file-service://", string.Empty);
                        var markdownImage = assetLocator.GetMarkdownImage(new AssetRequest(
                            searchPattern,
                            context.Role,
                            context.CreatedDate,
                            context.UpdatedDate)
                            );

                        if (string.IsNullOrWhiteSpace(markdownImage) == false)
                        {
                            markdownContent.Add(markdownImage);
                        }
                        else
                        {
                            Console.Error.WriteLine("\tUnable to find asset " + obj.asset_pointer);
                            markdownContent.Add($"> ⚠️ **Warning:** Could not find asset: {obj.asset_pointer}.");
                        }

                        if (context.MessageMetadata.image_gen_title != null)
                        {
                            markdownContent.Add($"*{context.MessageMetadata.image_gen_title}*  ");
                        }
                        markdownContent.Add($"**Size:** {obj.size_bytes} **Dims:** {obj.width}x{obj.height}  ");
                    }
                }
                else if (part.IsString)
                {
                    markdownContent.Add(part.StringValue);
                }
            }
            return new MarkdownContentResult(markdownContent);
        }

        public MarkdownContentResult Visit(ContentCode content, ContentVisitorContext context)
        {
            if (string.IsNullOrWhiteSpace(content.text))
            {
                return new MarkdownContentResult();
            }

            var searchRegex = SearchRegex();
            var matches = searchRegex.Match(content.text);
            if (content.language == "unknown" && matches.Success)
            {
                var code = matches.Groups[1].Value;
                return new MarkdownContentResult($"> 🔍 **Web search:** {code}.");
            }
            else if (content.language == "unknown" && content.text.IsValidJson())
            {
                var code = ToCodeBlock(content.text, "json");
                return new MarkdownContentResult(code);
            }
            else
            {
                var code = ToCodeBlock(content.text, content.language);
                return new MarkdownContentResult(code);
            }
        }

        private string ToCodeBlock(string code, string? language = null)
        {
            return $"```{language}{LineBreak}{code}{LineBreak}```";
        }

        public MarkdownContentResult Visit(ContentThoughts content, ContentVisitorContext context)
        {
            var markdownContent = new List<string>();
            foreach (var thought in content.thoughts)
            {
                markdownContent.Add(thought.summary + "  ");
                markdownContent.Add(thought.content + "  ");
            }
            return new MarkdownContentResult(markdownContent, " 💭");
        }

        public MarkdownContentResult Visit(ContentExecutionOutput content, ContentVisitorContext context)
        {
            var code = ToCodeBlock(content.text);
            return new MarkdownContentResult(code);
        }

        public MarkdownContentResult Visit(ContentReasoningRecap content, ContentVisitorContext context)
        {
            return new MarkdownContentResult(content.content);
        }

        public MarkdownContentResult Visit(ContentBase content, ContentVisitorContext context)
        {
            string name = content.GetType().Name;
            Console.WriteLine("\t" + name);
            return new MarkdownContentResult($"Unhandled content type: {content}");
        }

        private IEnumerable<string> DecodeText(string text, ContentVisitorContext context)
        {
            // image prompt
            if (text.Contains("prompt") && text.Contains("size"))
            {
                PromptFormat? pf = null;
                try
                {
                    var deserializedPromptFormat = JsonSerializer.Deserialize<PromptFormat>(text);
                    if (deserializedPromptFormat != null && deserializedPromptFormat.HasPrompt())
                    {
                        pf = deserializedPromptFormat;
                    }
                }
                catch (Exception ex)
                {
                }

                if (pf != null)
                {
                    yield return "> **Prompt:** " + pf.prompt;
                    yield return LineBreak;
                    yield return "> **Size:** " + pf.size;
                }
                else
                {
                    yield return text;
                }
            }
            // canvas create/update
            else if (context.Recipient.StartsWith("canmore"))
            {
                if (context.Recipient == "canmore.create_textdoc")
                {
                    var createCanvas = JsonSerializer.Deserialize<CanvasCreateModel>(text);
                    canvasContext = createCanvas;
                }
                else if (context.Recipient == "canmore.update_textdoc")
                {
                    var updateCanvas = JsonSerializer.Deserialize<CanvasUpdateModel>(text + "]}"); // for some reason the json isn't complete.
                    Debug.Assert(canvasContext != null);
                    canvasContext ??= new CanvasCreateModel() { type = "document " }; // default to document if no canvas exists

                    foreach (var update in updateCanvas.updates)
                    {
                        canvasContext.content = update.replacement;
                    }
                }

                if (canvasContext.type == "document")
                {
                    yield return canvasContext.content;
                }
                else if (canvasContext.type.StartsWith("code"))
                {
                    var language = canvasContext.type.Replace("code/", "");
                    yield return ToCodeBlock(canvasContext.content, language);
                }
                else
                {
                    yield return text;
                }
            }
            else
            {
                yield return text;
            }
        }

        private class PromptFormat
        {
            public string? prompt { get; set; }
            public string? size { get; set; }

            public bool HasPrompt() => string.IsNullOrWhiteSpace(prompt) == false && string.IsNullOrWhiteSpace(size) == false;
        }

        [GeneratedRegex("""^search\("(.*)"\)$""")]
        private static partial Regex SearchRegex();
    }
}
