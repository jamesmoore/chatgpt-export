using ChatGPTExport.Formatters.Html;

namespace ChatGTPExportTests.Formatters.Html
{
    public class MarkdownMathConverterTests
    {
        [Fact]
        public void PlainInlineMath_GetsDoubleEscaped()
        {
            var md = @"Area is \(\pi r^2\).";
            var actual = MarkdownMathConverter.ConvertBackslashMathToDollar(md);
            Assert.Equal(@"Area is $\pi r^2$.", actual);
        }

        [Fact]
        public void PlainDisplayMath_GetsDoubleEscaped()
        {
            var md = @"See: \[E=mc^2\]";
            var actual = MarkdownMathConverter.ConvertBackslashMathToDollar(md);
            Assert.Equal(@"See: $$E=mc^2$$", actual);
        }

        [Fact]
        public void NoMath_RemainsUnchanged()
        {
            var md = "This has no math delimiters, only text and (parentheses) and [brackets].";
            var actual = MarkdownMathConverter.ConvertBackslashMathToDollar(md);
            Assert.Equal(md, actual);
        }

        [Fact]
        public void InlineCode_IsNotModified()
        {
            var md = @"Use regex: `\(foo\)` and `\[bar\]`.";
            var actual = MarkdownMathConverter.ConvertBackslashMathToDollar(md);
            Assert.Equal(md, actual);
        }

        [Fact]
        public void FencedCodeBlock_IsNotModified()
        {
            var md = """
                 Before \(x\)

                 ```csharp
                 var pattern = @"\\(foo\\) \\[bar\\]";
                 // math-like text here should not be touched
                 ```
                 
                 After \[y\]
                 """;
            var expected = """
                       Before $x$

                       ```csharp
                       var pattern = @"\\(foo\\) \\[bar\\]";
                       // math-like text here should not be touched
                       ```
                       
                       After $$y$$
                       """;
            var actual = MarkdownMathConverter.ConvertBackslashMathToDollar(md);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FencedTildeBlock_IsNotModified()
        {
            var md = """
                 ~~~
                 literal \[not math\] \(still not math\)
                 ~~~
                 Outside: \(real math\)
                 """;
            var expected = """
                       ~~~
                       literal \[not math\] \(still not math\)
                       ~~~
                       Outside: $real math$
                       """;
            var actual = MarkdownMathConverter.ConvertBackslashMathToDollar(md);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InlineCode_WithBackticksInside_IsNotModified()
        {
            // CommonMark allows inline code like ``code with ` backtick``
            var md = @"Example: ``code with ` backtick`` and text \(x\).";
            var expected = @"Example: ``code with ` backtick`` and text $x$.";
            var actual = MarkdownMathConverter.ConvertBackslashMathToDollar(md);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MixedContent_OnlyNonCodeTextIsEscaped()
        {
            var md = """
                 Title: \(a+b\)
                 `inline \(should not change\)`
                 Paragraph with \[matrix\] and code:

                 ```txt
                 block: \(do not touch\)
                 ```
                 Tail \(t\)
                 """;
            var expected = """
                       Title: $a+b$
                       `inline \(should not change\)`
                       Paragraph with $$matrix$$ and code:

                       ```txt
                       block: \(do not touch\)
                       ```
                       Tail $t$
                       """;
            var actual = MarkdownMathConverter.ConvertBackslashMathToDollar(md);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void HandlesCRLF_Newlines()
        {
            var md = "Line1 \\(x\\)\r\n\r\n```js\r\nconst s = \"\\(nope\\)\";\r\n```\r\n\r\nEnd \\[y\\]";
            var expected = "Line1 $x$\r\n\r\n```js\r\nconst s = \"\\(nope\\)\";\r\n```\r\n\r\nEnd $$y$$";
            var actual = MarkdownMathConverter.ConvertBackslashMathToDollar(md);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Idempotent_WhenRunTwice()
        {
            var md = @"Mix \(x\) and \[y\], plus `\(code\)`.";
            var once = MarkdownMathConverter.ConvertBackslashMathToDollar(md);
            var twice = MarkdownMathConverter.ConvertBackslashMathToDollar(once);
            Assert.Equal(once, twice); // Safe to call multiple times
        }

        [Fact]
        public void CodeBlockWithInfoString_IsNotModifiedInside()
        {
            var md = """
                 ```csharp
                 // \(should not be escaped\)
                 string s = "\\(keep single backslashes\\)";
                 ```
                 Outside \(do escape\)
                 """;
            var expected = """
                       ```csharp
                       // \(should not be escaped\)
                       string s = "\\(keep single backslashes\\)";
                       ```
                       Outside $do escape$
                       """;
            var actual = MarkdownMathConverter.ConvertBackslashMathToDollar(md);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BracketsAndParensWithoutBackslash_Unchanged()
        {
            var md = "This (is not math) and [neither is this].";
            var actual = MarkdownMathConverter.ConvertBackslashMathToDollar(md);
            Assert.Equal(md, actual);
        }
    }
}