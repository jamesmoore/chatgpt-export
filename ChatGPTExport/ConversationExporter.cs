using System.Buffers;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using ChatGPTExport.Assets;
using ChatGPTExport.Formatters;
using ChatGPTExport.Models;

namespace ChatGPTExport
{
    public class ConversationExporter(IFileSystem fileSystem, IEnumerable<IConversationFormatter> exporters, ExportMode exportMode)
    {
        /// <summary>
        /// Processes an instance of a conversation.
        /// </summary>
        /// <param name="conversations">Conversation.</param>
        /// <param name="destination">Destination directory.</param>
        /// <param name="assetLocator">The asset locator.</param>
        /// <exception cref="ApplicationException"></exception>
        public void Process(Conversation conversation, IDirectoryInfo destination, IAssetLocator assetLocator)
        {
            try
            {
                var fileContentsMap = new Dictionary<string, IEnumerable<string>>();

                Console.WriteLine(conversation.title);

                Console.WriteLine($"\tMessages: {conversation.mapping!.Count}\tLeaves: {conversation.mapping.Count(p => p.Value.IsLeaf())}");

                var conversationToExport = exportMode == ExportMode.Complete ? conversation : conversation.GetLastestConversation();
                foreach (var exporter in exporters)
                {
                    Console.Write($"\t\t{exporter.GetType().Name}");
                    var exportFilename = GetFilename(conversationToExport, "", exporter.GetExtension());
                    ExportConversation(fileContentsMap, assetLocator, exporter, conversationToExport, exportFilename);
                    Console.WriteLine($"...Done");
                }

                foreach (var kv in fileContentsMap)
                {
                    var destinationFilename = fileSystem.Path.Join(destination.FullName, kv.Key);
                    var contents = string.Join(Environment.NewLine, kv.Value);
                    var destinationExists = fileSystem.File.Exists(destinationFilename);
                    if (destinationExists == false || destinationExists && FileStringMismatch(destinationFilename, contents))
                    {
                        fileSystem.File.WriteAllText(destinationFilename, contents);
                        fileSystem.File.SetCreationTimeUtcIfPossible(destinationFilename, conversation.GetCreateTime().DateTime);
                        fileSystem.File.SetLastWriteTimeUtc(destinationFilename, conversation.GetUpdateTime().DateTime);
                        Console.WriteLine($"\t{kv.Key}...Saved");
                    }
                    else
                    {
                        Console.WriteLine($"\t{kv.Key}...No change");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }


        private bool FileStringMismatch(string destinationFilename, string contents)
        {
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            var fileInfo = fileSystem.FileInfo.New(destinationFilename);
            if (fileInfo.Length != encoding.GetByteCount(contents))
            {
                return true;
            }

            using var stream = fileSystem.File.OpenRead(destinationFilename);
            using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 4096);
            var buffer = ArrayPool<char>.Shared.Rent(4096);
            try
            {
                var offset = 0;
                int read;
                while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (offset + read > contents.Length)
                    {
                        return true;
                    }

                    if (!contents.AsSpan(offset, read).SequenceEqual(buffer.AsSpan(0, read)))
                    {
                        return true;
                    }

                    offset += read;
                }

                return offset != contents.Length;
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        private static void ExportConversation(Dictionary<string, IEnumerable<string>> fileContentsMap, IAssetLocator assetLocator, IConversationFormatter exporter, Conversation conversation, string filename)
        {
            try
            {
                fileContentsMap[filename] = exporter.Format(assetLocator, conversation);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private string GetFilename(Conversation conversation, string modifier, string extension)
        {
            var createtime = conversation.GetCreateTime();
            var value = $"{createtime:yyyy-MM-ddTHH-mm-ss} - {conversation.title}{(string.IsNullOrWhiteSpace(modifier) ? "" : $" - {modifier}")}";
            value = new string(value.Where(p => fileSystem.Path.GetInvalidFileNameChars().Contains(p) == false).ToArray());
            return value.Trim() + extension;
        }
    }
}

