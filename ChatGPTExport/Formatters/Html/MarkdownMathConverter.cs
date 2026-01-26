using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace ChatGPTExport.Formatters.Html
{
    public static class MarkdownMathConverter
    {
        // Reuse a single pipeline; UsePreciseSourceLocation is required for SourceSpan.
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UsePreciseSourceLocation()
            .Build();

        /// <summary>
        /// Converts backslash-delimited MathJax markers to dollar notation:
        ///   \( -> $ ,  \) -> $
        ///   \[ -> $$,  \] -> $$
        /// Skips text inside inline code spans and fenced/indented code blocks.
        /// No pairing validation; converts delimiters literally.
        /// </summary>
        public static string ConvertBackslashMathToDollar(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return markdown ?? string.Empty;

            var doc = Markdig.Markdown.Parse(markdown, Pipeline);

            // Collect protected spans (code of any kind).
            var protectedSpans = doc
                .Descendants()
                .Where(p => p is CodeInline || p is FencedCodeBlock || p is CodeBlock)
                .Where(p => p.Span.Start >= 0 && p.Span.End >= p.Span.Start)
                .Select(p => (start: p.Span.Start, end: p.Span.End))
                .ToList();

            // Merge overlaps/adjacent
            protectedSpans.Sort((a, b) => a.start.CompareTo(b.start));
            var merged = new List<(int start, int end)>(protectedSpans.Count);
            foreach (var r in protectedSpans)
            {
                if (merged.Count == 0 || r.start > merged[^1].end + 1)
                    merged.Add(r);
                else
                    merged[^1] = (merged[^1].start, Math.Max(merged[^1].end, r.end));
            }

            bool HasProtection = merged.Count > 0;

            bool IsProtected(int idx)
            {
                if (!HasProtection) return false;
                int lo = 0, hi = merged.Count - 1;
                while (lo <= hi)
                {
                    int mid = lo + ((hi - lo) >> 1);
                    var (s, e) = merged[mid];
                    if (idx < s) hi = mid - 1;
                    else if (idx > e) lo = mid + 1;
                    else return true;
                }
                return false;
            }

            // Single pass over the original text, replacing only when not in a protected span.
            var src = markdown.AsSpan();
            var sb = new StringBuilder(markdown.Length + 32);

            int i = 0;
            while (i < src.Length)
            {
                if (src[i] == '\\' && i + 1 < src.Length && !IsProtected(i))
                {
                    char n1 = src[i + 1];
                    if (n1 == '(' || n1 == ')')
                    {
                        sb.Append('$');
                        i += 2;
                        continue;
                    }
                    if (n1 == '[' || n1 == ']')
                    {
                        sb.Append("$$");
                        i += 2;
                        continue;
                    }
                }

                sb.Append(src[i]);
                i++;
            }

            return sb.ToString();
        }
    }
}