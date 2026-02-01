using System.IO;
using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    public class ConversationAssets
    {
        public static ConversationAssets FromDirectory(IDirectoryInfo parentDirectory)
        {
            return new ConversationAssets(parentDirectory);
        }

        private readonly IDirectoryInfo parentDirectory;
        private string[]? cachedFiles;

        private ConversationAssets(IDirectoryInfo parentDirectory)
        {
            this.parentDirectory = parentDirectory;
        }

        public string? GetAsset(string searchPattern)
        {
            // Lazy initialization: only enumerate files on first access
            if (cachedFiles == null)
            {
                // Use EnumerateFiles to avoid materializing the entire list up front
                cachedFiles = parentDirectory.FileSystem.Directory
                    .EnumerateFiles(parentDirectory.FullName, "*", SearchOption.AllDirectories)
                    .ToArray();
            }

            return cachedFiles.FirstOrDefault(p => p.Contains(searchPattern));
        }
    }
}
