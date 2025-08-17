using System.Data;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.RegularExpressions;
using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    internal partial class ContentVisitor(IFileSystem fileSystem, IDirectoryInfo sourceDirectory, IDirectoryInfo destinationDirectory) : IContentVisitor<MarkdownContentResult>
    {
        private readonly string LineBreak = Environment.NewLine;

        public MarkdownContentResult Visit(ContentText content, ContentVisitorContext context)
        {
            var parts = content.parts.Where(TextContentFilter).SelectMany(SeparatePromptIfPresent).ToList();

            var content_references = context.MessageMetadata.content_references;
            if (content_references != null && content_references.Length != 0)
            {
                var textPart = parts[0];

                var sourcesFootnote = content_references.Where(p => p.type == "sources_footnote").FirstOrDefault();

                var reversed = content_references.OrderByDescending(p => p.start_idx).ToList();

                if (sourcesFootnote != null) {
                    var footnote = reversed.First();
                    Debug.Assert(footnote == sourcesFootnote);
                }

                var groupedWebpagesItems = content_references.Where(p => p.type == "grouped_webpages").SelectMany(p => p.items).ToList();

                var reindexedElements = textPart.GetRenderedElementIndexes();

                foreach (var contentReference in reversed)
                {
                    int start_idx = contentReference.start_idx < reindexedElements.Count ? reindexedElements[contentReference.start_idx] : contentReference.start_idx;
                    int end_idx = contentReference.end_idx < reindexedElements.Count ? reindexedElements[contentReference.end_idx] : contentReference.end_idx;
                    switch (contentReference.type)
                    {
                        case "attribution":
                            break;
                        case "hidden":
                        case "grouped_webpages_model_predicted_fallback":
                        case "image_v2":
                        case "tldr":
                        case "products":
                            var products = "";
                            parts[0] = parts[0].Substring(0, start_idx) + products + parts[0].Substring(end_idx);
                            break;
                        case "nav_list":
                            var refHighlight2 = "";
                            parts[0] = parts[0].Substring(0, start_idx) + refHighlight2 + parts[0].Substring(end_idx);
                            break;
                        case "video":
                            var videolink = $"[![{contentReference.title}]({contentReference.thumbnail_url})]({contentReference.url.Replace("&utm_source=chatgpt.com", "")} \"{contentReference.title}\")";
                            parts[0] = parts[0].Substring(0, start_idx) + videolink + parts[0].Substring(end_idx);
                            break;
                        case "grouped_webpages":
                            var refHighlight = string.Join("", contentReference.items.Select(p => $"[^{groupedWebpagesItems.IndexOf(p) + 1}]").ToArray());
                            parts[0] = parts[0].Substring(0, start_idx) + refHighlight + parts[0].Substring(end_idx);
                            break;
                        case "sources_footnote":
                            break;
                        case "product_entity":
                            var productsEntity = contentReference.alt;
                            parts[0] = parts[0].Substring(0, start_idx) + productsEntity + parts[0].Substring(end_idx);
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
            var typedContent = content;
            var markdownContent = new List<string>();
            foreach (var part in typedContent.parts)
            {
                if (part.IsObject)
                {
                    var obj = part.ObjectValue;
                    if (obj.content_type == "image_asset_pointer" && string.IsNullOrWhiteSpace(obj.asset_pointer) == false)
                    {
                        var searchPattern = obj.asset_pointer.Replace("sediment://", string.Empty).Replace("file-service://", string.Empty) + "*.*";
                        var markdownImage =
                            FindAssetInSourceDirectory(searchPattern, context.Role, context.CreatedDate, context.UpdatedDate) ??
                            FindAssetInDestinationDirectory(searchPattern);

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

        private string? FindAssetInSourceDirectory(string searchPattern, string role, DateTimeOffset? createdDate, DateTimeOffset? updatedDate)
        {
            var files = fileSystem.Directory.GetFiles(sourceDirectory.FullName, searchPattern, System.IO.SearchOption.AllDirectories);
            Debug.Assert(files.Length <= 1); // There shouldn't be more than one file.
            var file = files.FirstOrDefault();
            if (file != null)
            {
                var withoutPath = fileSystem.Path.GetFileName(file);
                var assetsPath = $"{role}-assets";
                var assetsDir = fileSystem.Path.Join(destinationDirectory.FullName, assetsPath);
                if (fileSystem.Directory.Exists(assetsDir) == false)
                {
                    fileSystem.Directory.CreateDirectory(assetsDir);
                }

                var fullAssetPath = fileSystem.Path.Combine(assetsDir, withoutPath);

                if (fileSystem.File.Exists(fullAssetPath) == false)
                {
                    fileSystem.File.Copy(file, fullAssetPath, true);

                    if (createdDate.HasValue)
                    {
                        fileSystem.File.SetCreationTimeUtcIfPossible(fullAssetPath, createdDate.Value.DateTime);
                    }

                    if (updatedDate.HasValue)
                    {
                        fileSystem.File.SetLastWriteTimeUtc(fullAssetPath, updatedDate.Value.DateTime);
                    }
                }

                return $"![{withoutPath}](./{assetsPath}/{withoutPath})  ";
            }

            return null;
        }

        private string? FindAssetInDestinationDirectory(string searchPattern)
        {
            // it may already exist in the destination directory from a previous export 
            var destinationMatches = fileSystem.Directory.GetFiles(destinationDirectory.FullName, searchPattern, System.IO.SearchOption.AllDirectories);
            if (destinationMatches.Length != 0)
            {
                var targetFile = fileSystem.FileInfo.New(destinationMatches.First());
                var relativePath = fileSystem.GetRelativePathTo(destinationDirectory, targetFile);
                if (fileSystem.Path.DirectorySeparatorChar != '/')
                {
                    relativePath = relativePath.Replace(fileSystem.Path.DirectorySeparatorChar, '/');
                }
                return $"![{targetFile.Name}](./{relativePath})  ";
            }
            return null;
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
                var code = $"```json{LineBreak}{content.text}{LineBreak}```";
                return new MarkdownContentResult(code);
            }
            else
            {
                var code = $"```{content.language}{LineBreak}{content.text}{LineBreak}```";
                return new MarkdownContentResult(code);
            }
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
            var code = $"```{LineBreak}{content.text}{LineBreak}```";
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

        private IEnumerable<string> SeparatePromptIfPresent(string p)
        {
            if (p.Contains("prompt") == false || p.Contains("size") == false)
            {
                yield return p;
            }
            else
            {
                PromptFormat pf = null;
                try
                {
                    var deserializedPromptFormat = JsonSerializer.Deserialize<PromptFormat>(p);
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
                    yield return p;
                }
            }
        }

        private class PromptFormat
        {
            public string prompt { get; set; }
            public string size { get; set; }

            public bool HasPrompt() => string.IsNullOrWhiteSpace(prompt) == false && string.IsNullOrWhiteSpace(size) == false;
        }

        [GeneratedRegex("""^search\("(.*)"\)$""")]
        private static partial Regex SearchRegex();
    }
}
