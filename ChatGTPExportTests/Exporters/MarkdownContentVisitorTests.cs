using ChatGPTExport.Assets;
using ChatGPTExport.Exporters;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Exporters;

public class MarkdownContentVisitorTests
{
    private static MarkdownContentVisitor CreateVisitor()
    {
        return new MarkdownContentVisitor(new NullAssetLocator(), showHidden: false);
    }

    private static ContentVisitorContext CreateContext(string role)
    {
        return new ContentVisitorContext(role, null, null, new MessageMetadata(), string.Empty);
    }

    [Fact]
    public void UserMessages_AreSanitized()
    {
        var visitor = CreateVisitor();
        var content = new ContentText { parts = ["<script>alert('x')</script>"] };
        var context = CreateContext("user");

        var result = visitor.Visit(content, context);

        var line = Assert.Single(result.Lines);
        Assert.Equal("&lt;script&gt;alert('x')&lt;/script&gt;", line);
    }

    [Fact]
    public void NonUserMessages_AreNotSanitized()
    {
        var visitor = CreateVisitor();
        var content = new ContentText { parts = ["<script>alert('x')</script>"] };
        var context = CreateContext("assistant");

        var result = visitor.Visit(content, context);

        var line = Assert.Single(result.Lines);
        Assert.Equal("<script>alert('x')</script>", line);
    }

    private class NullAssetLocator : IAssetLocator
    {
        public string? GetMarkdownMediaAsset(AssetRequest assetRequest) => null;
    }
}
