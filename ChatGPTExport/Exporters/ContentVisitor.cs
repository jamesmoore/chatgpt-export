using System.Data;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Text.Json;
using ChatGPTExport.Models;

namespace ChatGPTExport.Exporters
{
    internal class ContentVisitor(IFileSystem fileSystem, IDirectoryInfo sourceDirectory, IDirectoryInfo destinationDirectory) : IContentVisitor<MarkdownContentResult>
    {
        private readonly string LineBreak = Environment.NewLine;

        public MarkdownContentResult Visit(ContentText content, ContentVisitorContext context)
        {
            var parts = content.parts.Where(TextContentFilter).SelectMany(SeparatePromptIfPresent).ToList();
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
            if (destinationMatches.Any())
            {
                var relativePath = fileSystem.GetRelativePathTo(destinationDirectory, fileSystem.FileInfo.New(destinationMatches.First()));
                var withoutPath = fileSystem.Path.GetFileName(relativePath);
                return $"![{withoutPath}]({relativePath})  ";
            }
            return null;
        }

        public MarkdownContentResult Visit(ContentCode content, ContentVisitorContext context)
        {
            var code = $"```{content.language}{LineBreak}{content.text}{LineBreak}```";
            return new MarkdownContentResult([code]);
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
            return new MarkdownContentResult([code]);
        }

        public MarkdownContentResult Visit(ContentReasoningRecap content, ContentVisitorContext context)
        {
            return new MarkdownContentResult([content.content]);
        }

        public MarkdownContentResult Visit(ContentBase content, ContentVisitorContext context)
        {
            string name = content.GetType().Name;
            Console.WriteLine("\t" + name);
            return new MarkdownContentResult([$"Unhandled content type: {content}"]);
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
    }
}
