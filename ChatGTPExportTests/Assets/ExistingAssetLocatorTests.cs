using ChatGPTExport.Assets;
using System.IO.Abstractions.TestingHelpers;

namespace ChatGTPExportTests.Assets
{
    public class ExistingAssetLocatorTests
    {
        [Theory]
        [InlineData("image*")]
        [InlineData("image?")]
        [InlineData("image/")]
        [InlineData("image\\")]
        [InlineData("invali<d")]
        public void GetMarkdownMediaAsset_InvalidPattern_ReturnsNull(string pattern)
        {
            var fs = new MockFileSystem();
            var destPath = fs.Path.Combine(fs.Path.DirectorySeparatorChar.ToString(), "dest");
            fs.AddDirectory(destPath);
            var destDir = fs.DirectoryInfo.New(destPath);
            var locator = new ExistingAssetLocator(fs, destDir);

            var request = new AssetRequest(pattern, "role", null, null);
            var result = locator.GetMarkdownMediaAsset(request);

            Assert.Null(result);
        }

        [Fact]
        public void GetMarkdownMediaAsset_ValidPattern_ReturnsMarkdown()
        {
            var fs = new MockFileSystem();
            var destPath = fs.Path.Combine(fs.Path.DirectorySeparatorChar.ToString(), "dest");
            fs.AddDirectory(destPath);
            var filePath = fs.Path.Combine(destPath, "image1.png");
            fs.AddFile(filePath, new MockFileData("data"));

            var destDir = fs.DirectoryInfo.New(destPath);
            var locator = new ExistingAssetLocator(fs, destDir);

            var request = new AssetRequest("image1", "role", null, null);
            var result = locator.GetMarkdownMediaAsset(request);

            Assert.NotNull(result);
            Assert.Contains("image1.png", result);
        }
    }
}
