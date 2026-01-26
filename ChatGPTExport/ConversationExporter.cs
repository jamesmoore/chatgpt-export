using System.Buffers;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using ChatGPTExport.Assets;
using ChatGPTExport.Exporters;
using ChatGPTExport.Models;

namespace ChatGPTExport
{
    public class ConversationExporter(IFileSystem fileSystem, IEnumerable<IExporter> exporters, ExportMode exportMode)
    {
        /// <summary>
        /// Processes multiple instances of the same conversation.
        /// Why do we process all conversations instead of just the latest instance? 
        ///     There may be assets in older exports that are absent in newer ones. 
        ///     Processing the older ones enables them to be transferred to the destination directory.
        /// </summary>
        /// <param name="conversations">Enumerable tuple of asset locator and corresponding conversation (Must be in date order).</param>
        /// <param name="destination">Destination directory.</param>
        /// <exception cref="ApplicationException"></exception>
        public void Process(IEnumerable<(IAssetLocator AssetLocator, Conversation conversation)> conversations, IDirectoryInfo destination)
        {
            try
            {
                if (conversations.Select(p => p.conversation.conversation_id).Distinct().Count() > 1)
                {
                    throw new ApplicationException("Unable to export instances of multiple different conversations at once");
                }

                var fileContentsMap = new Dictionary<string, IEnumerable<string>>();

                var conversationsInDateOrder = conversations.OrderBy(p => p.conversation.update_time).ToList();
                var titles = string.Join(Environment.NewLine, conversationsInDateOrder.Select(p => p.conversation.title).Distinct().ToArray());
                Console.WriteLine(titles);

                foreach (var (assetLocator, conversation) in conversationsInDateOrder)
                {
                    if (conversation.mapping != null)
                    {
                        Console.WriteLine($"\tMessages: {conversation.mapping.Count}\tLeaves: {conversation.mapping.Count(p => p.Value.IsLeaf())}");
                        foreach (var exporter in exporters)
                        {
                            Console.Write($"\t\t{exporter.GetType().Name}");

                            if (exportMode == ExportMode.Complete)
                            {
                                var completeFilename = GetFilename(conversation, "", exporter.GetExtension());
                                ExportConversation(fileContentsMap, assetLocator, exporter, conversation, completeFilename);
                            }
                            else if (exportMode == ExportMode.Latest)
                            {
                                var latest = conversation.GetLastestConversation();
                                var filename = GetFilename(latest, "", exporter.GetExtension());
                                ExportConversation(fileContentsMap, assetLocator, exporter, latest, filename);
                            }

                            Console.WriteLine($"...Done");
                        }
                    }
                }

                foreach (var kv in fileContentsMap)
                {
                    var destinationFilename = fileSystem.Path.Join(destination.FullName, kv.Key);
                    var contents = string.Join(Environment.NewLine, kv.Value);
                    var destinationExists = fileSystem.File.Exists(destinationFilename);
                    if (destinationExists == false || destinationExists && FileStringMismatch(destinationFilename, contents))
                    {
                        fileSystem.File.WriteAllText(destinationFilename, contents);
                        var lastConversation = conversationsInDateOrder.Last().conversation;
                        fileSystem.File.SetCreationTimeUtcIfPossible(destinationFilename, lastConversation.GetCreateTime().DateTime);
                        fileSystem.File.SetLastWriteTimeUtc(destinationFilename, lastConversation.GetUpdateTime().DateTime);
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

        private static void ExportConversation(Dictionary<string, IEnumerable<string>> fileContentsMap, IAssetLocator assetLocator, IExporter exporter, Conversation conversation, string filename)
        {
            try
            {
                fileContentsMap[filename] = exporter.Export(assetLocator, conversation);
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

