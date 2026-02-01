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
        private readonly Lazy<string[]> cachedFiles;

        private ConversationAssets(IDirectoryInfo parentDirectory)
        {
            this.parentDirectory = parentDirectory;
            // Use Lazy<T> for thread-safe lazy initialization
            this.cachedFiles = new Lazy<string[]>(() =>
                parentDirectory.FileSystem.Directory
                    .EnumerateFiles(parentDirectory.FullName, "*", SearchOption.AllDirectories)
                    .ToArray()
            );
        }

        public string? GetAsset(string searchPattern)
        {
            // Lazy initialization: only enumerate files on first access
            return cachedFiles.Value.FirstOrDefault(p => p.Contains(searchPattern));
        }
    }
}
