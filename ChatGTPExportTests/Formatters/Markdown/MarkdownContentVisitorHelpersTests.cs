using ChatGPTExport.Formatters.Markdown;

namespace ChatGTPExportTests.Formatters.Markdown;

public class MarkdownContentVisitorHelpersTests
{
    [Fact]
    public void SanitizeUserInputMarkdown_EscapesRelevantTagsOutsideCode()
    {
        var input = "<script>alert('x')</script>";

        var result = MarkdownContentVisitorHelpers.SanitizeUserInputMarkdown(input);

        Assert.Equal("&lt;script&gt;alert('x')&lt;/script&gt;", result);
    }

    [Fact]
    public void SanitizeUserInputMarkdown_DoesNotEscapeInsideInlineCode()
    {
        var input = "Use `<script>` here";

        var result = MarkdownContentVisitorHelpers.SanitizeUserInputMarkdown(input);

        Assert.Equal("Use `<script>` here", result);
    }

    [Fact]
    public void SanitizeUserInputMarkdown_DoesNotEscapeInFencedBlock()
    {
        var input = "```\n<script>alert('x')</script>\n```";

        var result = MarkdownContentVisitorHelpers.SanitizeUserInputMarkdown(input);

        Assert.Equal(input, result);
    }

    [Fact]
    public void SanitizeUserInputMarkdown_DoesNotEscapeInPreformattedLine()
    {
        var input = "    <script>alert('x')</script>";

        var result = MarkdownContentVisitorHelpers.SanitizeUserInputMarkdown(input);

        Assert.Equal(input, result);
    }

    [Fact]
    public void SanitizeUserInputMarkdown_ReformatsLineEndings_WhenSafe()
    {
        var input = "line1\nline2";

        var result = MarkdownContentVisitorHelpers.SanitizeUserInputMarkdown(input);

        Assert.Equal("line1  \nline2", result);
    }

    [Fact]
    public void SanitizeUserInputMarkdown_DoesNotReformat_WhenNextLineEmpty()
    {
        var input = "line1\n\nline2";

        var result = MarkdownContentVisitorHelpers.SanitizeUserInputMarkdown(input);

        Assert.Equal(input, result);
    }
}
