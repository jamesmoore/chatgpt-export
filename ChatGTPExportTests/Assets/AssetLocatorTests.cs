using ChatGPTExport.Assets;
using System.IO.Abstractions.TestingHelpers;

namespace ChatGTPExportTests.Assets
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
            var existing = new ExistingAssetLocator(fs, destDir);
            var locator = new AssetLocator(fs, sourceDir, destDir, existing);

            var request = new AssetRequest("img.png", "../evil", null, null);

            var result = locator.GetMarkdownMediaAsset(request);

            var expected = fs.Path.Combine(destDir.FullName, "unknown-assets", "img.png");
            Assert.True(fs.File.Exists(expected));
            Assert.Equal("![img.png](./unknown-assets/img.png)  ", result.GetMarkdownLink());

            var traversal = fs.Path.Combine(destDir.FullName, "..", "evil-assets", "img.png");
            Assert.False(fs.File.Exists(traversal));
        }
    }
}
