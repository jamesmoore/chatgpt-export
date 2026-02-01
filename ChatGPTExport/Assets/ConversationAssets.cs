using System.IO;
using System.IO.Abstractions;

namespace ChatGPTExport.Assets
{
    public class ConversationAssets
    {
        public static ConversationAssets FromDirectory(IDirectoryInfo parentDirectory)
        {
            var files = parentDirectory.FileSystem.Directory.GetFiles(parentDirectory.FullName, "*", SearchOption.AllDirectories);
            return new ConversationAssets(parentDirectory, files);
        }

        private readonly IDirectoryInfo parentDirectory;
        private readonly string[] files;

        private ConversationAssets(IDirectoryInfo parentDirectory, string[] files)
        {
            this.parentDirectory = parentDirectory;
            this.files = files;
        }

        public string? GetAsset(string searchPattern)
        {
            return files.FirstOrDefault(p => p.Contains(searchPattern));
        }
    }
}
