using ChatGpt.Exporter.Cli.Assets;
using ChatGPTExport.Assets;
using System.IO.Abstractions.TestingHelpers;

namespace ChatGpt.Exporter.Cli.Tests.Assets
{
    public class AssetLocatorTests
    {
        [Fact]
        public void Malformed_role_does_not_escape_destination()
        {
            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { MockUnixSupport.Path(@"c:\source\img.png"), new MockFileData("data") }
            });
            fs.AddDirectory(MockUnixSupport.Path(@"c:\dest"));

            var sourceDir = fs.DirectoryInfo.New(MockUnixSupport.Path(@"c:\source"));
            var destDir = fs.DirectoryInfo.New(MockUnixSupport.Path(@"c:\dest"));
            var existing = new ExistingAssetLocator(destDir);
            var locator = new AssetLocator(ConversationAssets.FromDirectory(sourceDir), destDir, existing);

            var request = new AssetRequest("img.png", "../evil", null, null);

            var result = locator.GetMarkdownMediaAsset(request);

            var expected = fs.Path.Combine(destDir.FullName, "unknown-assets", "img.png");
            Assert.True(fs.File.Exists(expected));
            
            Assert.NotNull(result);
            var markdownLink = result.GetMarkdownLink();
            Assert.Equal("![img.png](./unknown-assets/img.png)  ", markdownLink);

            var traversal = fs.Path.Combine(destDir.FullName, "..", "evil-assets", "img.png");
            Assert.False(fs.File.Exists(traversal));
        }
    }
}
