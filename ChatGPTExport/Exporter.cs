using System.IO.Abstractions;
using System.Text.Json;
using ChatGPTExport.Exporters;
using ChatGPTExport.Models;
using ChatGPTExport.Validators;

namespace ChatGPTExport
{
    public class Exporter(IDirectoryInfo source, IFileSystem fileSystem)
    {
        public void Process(IDirectoryInfo destination)
        {
            const string jsonPath = "conversations.json";
            var conversationsJsonPath = fileSystem.Path.Join(source.FullName, jsonPath);
            if (fileSystem.File.Exists(conversationsJsonPath))
            {
                var text = fileSystem.File.ReadAllText(conversationsJsonPath);

                var validator = new ContentTypeValidator();
                var unhandled = validator.GetUnhandledContentTypes(text);

                if(unhandled.Any())
                {
                    Console.Error.WriteLine("Warning - the the following conversations have unsupported content types:");
                    foreach (var unhandledContent in unhandled)
                    {
                        Console.WriteLine(unhandledContent.Title);
                        foreach(var contentType in unhandledContent.UnhandledContentTypes)
                        {
                            Console.WriteLine("\t" + contentType);
                        }
                    }
                }

                var conversations = JsonSerializer.Deserialize<Conversations>(text);

                // for round trip validation of the json schema
                var json2 = JsonSerializer.Serialize(conversations, new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                var differences = JsonComparer.CompareJson(text, json2);

                if (differences.Count != 0)
                {
                    Console.WriteLine("Found json schema discrepancies.");
                    foreach (var diff in differences)
                    {
                        Console.WriteLine(diff);
                    }
                }

                var exporters = new List<IExporter>()
                {
                    new JsonExporter(fileSystem, destination),
                    new MarkdownExporter(fileSystem, source, destination),
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
                throw new ApplicationException($"{jsonPath} not found");
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
