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

        public IDirectoryInfo ParentDirectory => parentDirectory;

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

        public IFileInfo? FindAsset(string searchPattern)
        {
            // Lazy initialization: only enumerate files on first access
            var path = cachedFiles.Value.FirstOrDefault(p => p.Contains(searchPattern));
            return path == null ? null : parentDirectory.FileSystem.FileInfo.New(path);
        }
    }
}
