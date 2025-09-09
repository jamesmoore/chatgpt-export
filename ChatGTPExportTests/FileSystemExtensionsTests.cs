using System.IO.Abstractions.TestingHelpers;
using ChatGPTExport;

namespace ChatGTPExportTests
{
    public class FileSystemExtensionsTests
    {
        private static MockFileSystem CreateFileSystem(bool caseSensitive)
        {
            var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            var files = new Dictionary<string, MockFileData>(comparer);
            return new MockFileSystem(files, "/");
        }

        private static void ResetCache()
        {
            typeof(FileSystemExtensions)
                .GetField("_isCaseSensitive", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(null, null);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsFileSystemCaseSensitive_ReturnsExpected(bool caseSensitive)
        {
            ResetCache();
            var fileSystem = CreateFileSystem(caseSensitive);
            var result = fileSystem.IsFileSystemCaseSensitive();
            Assert.Equal(caseSensitive, result);
        }
    }
}
