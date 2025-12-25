using ChatGPTExport.Exporters.Markdown;

namespace ChatGTPExportTests.Exporters.Markdown;

public class MarkdownContentVisitorHelpersTests
{
    [Fact]
    public void SanitizeMarkdown_EscapesRelevantTagsOutsideCode()
    {
        var input = "<script>alert('x')</script>";

        var result = MarkdownContentVisitorHelpers.SanitizeMarkdown(input);

        Assert.Equal("&lt;script&gt;alert('x')&lt;/script&gt;", result);
    }

    [Fact]
    public void SanitizeMarkdown_DoesNotEscapeInsideInlineCode()
    {
        var input = "Use `<script>` here";

        var result = MarkdownContentVisitorHelpers.SanitizeMarkdown(input);

        Assert.Equal("Use `<script>` here", result);
    }

    [Fact]
    public void SanitizeMarkdown_DoesNotEscapeInFencedBlock()
    {
        var input = "```\n<script>alert('x')</script>\n```";

        var result = MarkdownContentVisitorHelpers.SanitizeMarkdown(input);

        Assert.Equal(input, result);
    }

    [Fact]
    public void SanitizeMarkdown_DoesNotEscapeInPreformattedLine()
    {
        var input = "    <script>alert('x')</script>";

        var result = MarkdownContentVisitorHelpers.SanitizeMarkdown(input);

        Assert.Equal(input, result);
    }

    [Fact]
    public void SanitizeMarkdown_ReformatsLineEndings_WhenSafe()
    {
        var input = "line1\nline2";

        var result = MarkdownContentVisitorHelpers.SanitizeMarkdown(input);

        Assert.Equal("line1  \nline2", result);
    }

    [Fact]
    public void SanitizeMarkdown_DoesNotReformat_WhenNextLineEmpty()
    {
        var input = "line1\n\nline2";

        var result = MarkdownContentVisitorHelpers.SanitizeMarkdown(input);

        Assert.Equal(input, result);
    }
}
