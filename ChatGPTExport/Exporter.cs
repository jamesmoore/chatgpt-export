using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatGPTExport.Exporters;
using ChatGPTExport.Models;
using ChatGPTExport.Validators;

namespace ChatGPTExport
{
    public class Exporter(IFileInfo source, IFileSystem fileSystem)
    {
        public void Process(IDirectoryInfo destination)
        {
            if (source.Exists)
            {
                var conversationsJson = fileSystem.File.ReadAllText(source.FullName);

                VerifyContentTypes(conversationsJson);

                var conversations = JsonSerializer.Deserialize<Conversations>(conversationsJson);
                if (conversations == null)
                {
                    throw new ApplicationException($"Conversations file {source.FullName} could not be deserialized");
                }
                VerifyJsonSerialization(conversationsJson, conversations);

                var exporters = new List<IExporter>()
                {
                    new JsonExporter(fileSystem, destination),
                    new MarkdownExporter(fileSystem, source.Directory, destination),
                };

                foreach (var conversation in conversations)
                {
                    Console.WriteLine(conversation.title + "\tMessages: " + conversation.mapping.Count + "\tLeaves: " + conversation.mapping.Count(p => p.Value.IsLeaf()));

                    if (conversation.HasMultipleBranches())
                    {
                        foreach (var exporter in exporters)
                        {
                            var filename = GetFilename(conversation, "complete", exporter.GetExtension());
                            exporter.Export(conversation, filename);
                        }
                    }

                    var latest = conversation.GetLastestConversation();
                    foreach (var exporter in exporters)
                    {
                        var filename = GetFilename(latest, "", exporter.GetExtension());
                        exporter.Export(latest, filename);
                    }
                }
            }
            else
            {
                throw new ApplicationException($"{source.FullName} not found");
            }
        }

        private static void VerifyContentTypes(string text)
        {
            var validator = new ContentTypeValidator();
            var unhandled = validator.GetUnhandledContentTypes(text);

            if (unhandled.Any())
            {
                Console.Error.WriteLine("Warning - the the following conversations have unsupported content types:");
                foreach (var unhandledContent in unhandled)
                {
                    Console.WriteLine(unhandledContent.Title);
                    foreach (var contentType in unhandledContent.UnhandledContentTypes)
                    {
                        Console.WriteLine("\t" + contentType);
                    }
                }
            }
        }

        private static readonly JsonSerializerOptions options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Reserialize the deserialized json and compare it to the original, to check everything matches.
        /// </summary>
        /// <param name="originalJson"></param>
        /// <param name="conversations"></param>
        private static void VerifyJsonSerialization(string originalJson, Conversations conversations)
        {
            // for round trip validation of the json schema
            var reserialized = JsonSerializer.Serialize(conversations, options);

            var differences = JsonComparer.CompareJson(originalJson, reserialized);

            if (differences.Count != 0)
            {
                Console.WriteLine("Found json schema discrepancies.");
                foreach (var diff in differences)
                {
                    Console.WriteLine(diff);
                }
            }
        }

        private string GetFilename(Conversation conversation, string modifier, string extension)
        {
            var createtime = conversation.GetCreateTime();
            var value = createtime.ToString("yyyy-MM-ddTHH-mm-ss") + " - " + conversation.title + (string.IsNullOrWhiteSpace(modifier) ? "" : " - " + modifier);
            value = new string(value.Where(p => fileSystem.Path.GetInvalidFileNameChars().Contains(p) == false).ToArray());
            return value + extension;
        }
    }
}
