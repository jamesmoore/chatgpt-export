using ChatGPTExport.Assets;
using ChatGPTExport.Exporters.Markdown;
using ChatGPTExport.Models;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ChatGPTExport.Exporters
{
    public partial class MarkdownContentVisitor(IAssetLocator assetLocator, bool showHidden) : IContentVisitor<MarkdownContentResult>
    {
        private const string trackingSource = "?utm_source=chatgpt.com";
        private const string ImageAssetPointer = "image_asset_pointer";
        private readonly string LineBreak = Environment.NewLine;
        private CanvasCreateModel? canvasContext = null;

        /// <summary>
        /// Catch all for unhandled content types.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public MarkdownContentResult Visit(ContentBase content, ContentVisitorContext context)
        {
            string name = content.GetType().Name;
            Console.WriteLine("\tUnhandled content type: " + name);

            var lines = new List<string>
            {
                $"Unhandled content type: {content}"
            };

            if (content.ExtraData != null && content.ExtraData.Count != 0)
            {
                lines.Add("|Name|Value|");
                lines.Add("|---|---|");
                foreach (var item in content.ExtraData.Take(1))
                {
                    lines.Add("|" + item.Key + "|" + item.Value.GetRawText().Replace("\\n", "<br>") + "|");
                }
            }

            return new MarkdownContentResult(lines);
        }

        public MarkdownContentResult Visit(ContentText content, ContentVisitorContext context)
        {
            var parts = content.parts?.Where(TextContentFilter).SelectMany(p => DecodeText(p, context)).ToList() ?? [];

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
                    var replacement = GetContentReferenceReplacement(contentReference, groupedWebpagesItems);

                    if (replacement != null)
                    {
                        var start_idx = contentReference.start_idx < reindexedElements.Count ? reindexedElements[contentReference.start_idx] : contentReference.start_idx;
                        var end_idx = contentReference.end_idx < reindexedElements.Count ? reindexedElements[contentReference.end_idx] : contentReference.end_idx;
                        var firstPartSpan = parts[0].AsSpan();
                        var firstSpan = firstPartSpan[..start_idx];
                        var lastSpan = firstPartSpan[end_idx..];

                        parts[0] = string.Concat(firstSpan, replacement, lastSpan);
                    }
                }

                parts.Add(string.Empty);
                var footnotes = groupedWebpagesItems.Select((p, i) => $"[^{i + 1}]: [{p.title}]({p.url?.Replace(trackingSource, "")})  ");
                parts.AddRange(footnotes);

                if (sourcesFootnote != null)
                {
                    var existingUrls = groupedWebpagesItems.Select(p => p.url).ToArray();
                    var newSources = sourcesFootnote.sources?.Where(p => existingUrls.Contains(p.url) == false).ToList() ?? [];
                    if (newSources.Any())
                    {
                        parts.Add(string.Empty);
                        parts.Add("### Sources");
                        parts.AddRange(newSources.Select(source => $"* [{source.title}]({source.url?.Replace(trackingSource, "")})  "));
                    }
                }
            }

            return new MarkdownContentResult(parts);
        }

        private string? GetContentReferenceReplacement(MessageMetadata.Content_References contentReference, List<MessageMetadata.Content_References.Item> groupedWebpagesItems)
        {
            switch (contentReference.type)
            {
                case "attribution":
                case "sources_footnote":
                    return null;
                case "hidden":
                case "grouped_webpages_model_predicted_fallback":
                case "image_v2":
                case "tldr":
                case "nav_list":
                case "navigation":
                case "webpage_extended":
                case "image_inline":
                    return string.Empty;
                case "products":
                case "product_entity":
                case "alt_text":
                    return contentReference.alt;
                case "video":
                    var videolink = $"[![{contentReference.title}]({contentReference.thumbnail_url})]({contentReference.url?.Replace("&utm_source=chatgpt.com", "")} \"{contentReference.title}\")";
                    return videolink;
                case "grouped_webpages":
                    var refHighlight = string.Join("", contentReference.items?.Select(p => $"[^{groupedWebpagesItems.IndexOf(p) + 1}]").ToArray() ?? []);
                    return refHighlight;
                case "image_group":
                    var safe_urls = contentReference.safe_urls ?? [];
                    var images = safe_urls.Any() ?
                        LineBreak + "Image search results: " + LineBreak + string.Join(LineBreak, safe_urls.Select(p => "* " + p.Replace(trackingSource, "")).Distinct()) :
                        string.Empty;
                    return images;
                case "entity":
                    var entityInfo = contentReference.name;
                    var disambiguation = contentReference.extra_params?.disambiguation;
                    var entityInfoString = $"{entityInfo}{(string.IsNullOrWhiteSpace(disambiguation) == false ? $" ({disambiguation})" : "")}";
                    return entityInfoString;
                default:
                    Console.WriteLine($"Unhandled content reference type: {contentReference.type}");
                    return $"[{contentReference.type}]";
            }
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
            bool hasImage = false;
            foreach (var part in content.parts ?? [])
            {
                if (part.IsObject && part.ObjectValue != null)
                {
                    var mediaAssets = GetMarkdownMediaAsset(context, part.ObjectValue);
                    markdownContent.AddRange(mediaAssets);
                    hasImage = hasImage || part.ObjectValue.content_type == ImageAssetPointer;
                }
                else if (part.IsString && part.StringValue != null)
                {
                    markdownContent.Add(part.StringValue);
                }
            }
            return new MarkdownContentResult(markdownContent, null, hasImage);
        }

        private IEnumerable<string> GetMarkdownMediaAsset(ContentVisitorContext context, ContentMultimodalText.ContentMultimodalTextParts obj)
        {
            switch (obj.content_type)
            {
                case ImageAssetPointer when string.IsNullOrWhiteSpace(obj.asset_pointer) == false:
                    {
                        var searchPattern = GetSearchPattern(obj.asset_pointer);
                        var markdownImage = GetMediaAsset(context, searchPattern);

                        if (markdownImage != null)
                        {
                            yield return markdownImage.GetMarkdownLink();
                        }
                        else
                        {
                            Console.Error.WriteLine("\tUnable to find asset " + obj.asset_pointer);
                            yield return $"> ⚠️ **Warning:** Could not find asset: {obj.asset_pointer}.";
                        }

                        if (context.MessageMetadata.image_gen_title != null)
                        {
                            yield return $"*{context.MessageMetadata.image_gen_title}*  ";
                        }
                        yield return $"**Size:** {obj.size_bytes} **Dims:** {obj.width}x{obj.height}  ";
                        break;
                    }

                case "real_time_user_audio_video_asset_pointer" when string.IsNullOrWhiteSpace(obj.audio_asset_pointer?.asset_pointer) == false:
                    {
                        var searchPattern = GetSearchPattern(obj.audio_asset_pointer.asset_pointer);
                        var markdownAsset = GetMediaAsset(context, searchPattern);

                        yield return $"{markdownAsset?.GetMarkdownLink()}  ";
                        break;
                    }

                case "audio_asset_pointer" when string.IsNullOrWhiteSpace(obj.asset_pointer) == false:
                    {
                        var searchPattern = GetSearchPattern(obj.asset_pointer);
                        var markdownAsset = GetMediaAsset(context, searchPattern);

                        yield return $"{markdownAsset?.GetMarkdownLink()}  ";
                        break;
                    }

                case "audio_transcription" when string.IsNullOrWhiteSpace(obj.text) == false:
                    yield return $"*{obj.text}*  ";
                    break;
            }
        }

        private static string GetSearchPattern(string assetPointer)
        {
            return assetPointer.Replace("sediment://", string.Empty).Replace("file-service://", string.Empty);
        }

        private Asset? GetMediaAsset(ContentVisitorContext context, string searchPattern)
        {
            return assetLocator.GetMarkdownMediaAsset(new AssetRequest(
                searchPattern,
                context.Role,
                context.CreatedDate,
                context.UpdatedDate)
                );
        }

        public MarkdownContentResult Visit(ContentCode content, ContentVisitorContext context)
        {
            if (showHidden == false && context.Recipient != "all")
            {
                return new MarkdownContentResult();
            }

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
            return GetShowAllGuardedContentResult(() =>
            {
                var markdownContent = new List<string>();
                if (content.thoughts != null)
                {
                    foreach (var thought in content.thoughts)
                    {
                        markdownContent.Add(thought.summary + "  ");
                        markdownContent.Add(thought.content + "  ");
                    }
                }
                return new MarkdownContentResult(markdownContent, " 💭");
            });
        }

        public MarkdownContentResult Visit(ContentExecutionOutput content, ContentVisitorContext context)
        {
            return GetShowAllGuardedContentResult(() =>
            {
                if (content.text == null)
                {
                    return new MarkdownContentResult();
                }

                var code = ToCodeBlock(content.text);
                return new MarkdownContentResult(code);
            });
        }

        public MarkdownContentResult Visit(ContentReasoningRecap content, ContentVisitorContext context)
        {
            return GetShowAllGuardedContentResult(() =>
            {
                if (content.content == null)
                {
                    return new MarkdownContentResult();
                }
                return new MarkdownContentResult(content.content);
            });
        }

        private IEnumerable<string> DecodeText(string text, ContentVisitorContext context)
        {
            // image prompt
            if (text.Contains("\"prompt\":") && text.Contains("\"size\":"))
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
                    Console.WriteLine("Could not deserialize text to prompt {0}", ex);
                }

                if (pf != null)
                {
                    if (pf.prompt != null)
                    {
                        yield return "> **Prompt:** " + GetPrompt(pf.prompt);
                        yield return LineBreak;
                    }
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

                if (canvasContext?.type == "document")
                {
                    yield return canvasContext.content;
                }
                else if (canvasContext?.type.StartsWith("code") ?? false)
                {
                    var language = canvasContext.type.Replace("code/", "");
                    yield return ToCodeBlock(canvasContext.content, language);
                }
                else
                {
                    yield return text;
                }
            }
            else if (context.Role == "user")
            {
                yield return MarkdownContentVisitorHelpers.SanitizeUserInputMarkdown(text);
            }
            else
            {
                yield return text;
            }
        }

        private string? GetPrompt(string prompt)
        {
            var lines = prompt.Split('\n').Select(p => p.Trim()).Where(p => string.IsNullOrEmpty(p) == false);
            var concatenated = string.Join("  " + LineBreak, lines); // add double space so the quote comes out in a single block
            return concatenated;
        }

        private class PromptFormat
        {
            public string? prompt { get; set; }
            public string? size { get; set; }

            public bool HasPrompt() => string.IsNullOrWhiteSpace(prompt) == false && string.IsNullOrWhiteSpace(size) == false;
        }

        [GeneratedRegex("""^search\("(.*)"\)$""")]
        private static partial Regex SearchRegex();

        public MarkdownContentResult Visit(ContentUserEditableContext content, ContentVisitorContext context)
        {
            return GetShowAllGuardedContentResult(() =>
            {
                var markdownContent = new List<string>
                {
                    "**User profile:** " + content.user_profile + "  ",
                    "**User instructions:** " + content.user_instructions + "  "
                };
                return new MarkdownContentResult(markdownContent);
            });
        }

        public MarkdownContentResult Visit(ContentTetherBrowsingDisplay content, ContentVisitorContext context)
        {
            return GetShowAllGuardedContentResult(() =>
            {
                if (content.result == null)
                {
                    return new MarkdownContentResult();
                }
                string v = content.result.Replace("\n", "  \n");
                return new MarkdownContentResult([v, content.summary]);
            });
        }

        public MarkdownContentResult Visit(ContentComputerOutput content, ContentVisitorContext context)
        {
            return new MarkdownContentResult();
        }

        public MarkdownContentResult Visit(ContentSystemError content, ContentVisitorContext context)
        {
            return GetShowAllGuardedContentResult(() =>
            {
                return new MarkdownContentResult($"🔴 {content.name}: {content.text}");
            });
        }

        private MarkdownContentResult GetShowAllGuardedContentResult(Func<MarkdownContentResult> func)
        {
            return showHidden ? func() : new MarkdownContentResult();
        }
    }
}
