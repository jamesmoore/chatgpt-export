using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace ChatGPTExport.Exporters
{
    public static class MathjaxEscaper
    {
        /// <summary>
        /// Doubles backslashes for MathJax delimiters \(...\) and \[...\] in markdown text,
        /// but skips anything inside inline code, fenced code blocks, and indented code blocks.
        /// Uses Markdig only to find code regions (with precise source locations).
        /// </summary>
        public static string EscapeBackslashMathOutsideCode(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return markdown;

            // 1) Parse once to get code spans with source indices (do NOT rely on inline content)
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UsePreciseSourceLocation() // important: populate SourceSpan on nodes
                .Build();

            var doc = Markdown.Parse(markdown, pipeline);

            // Collect protected intervals [start, end] (inclusive) for all code nodes
            var protectedSpans = new List<(int start, int end)>();

            foreach (var node in doc.Descendants())
            {
                if (node is CodeInline || node is FencedCodeBlock || node is CodeBlock)
                {
                    var span = node.Span; // SourceSpan with Start/End indices into the ORIGINAL text
                    if (span.Start >= 0 && span.End >= span.Start)
                        protectedSpans.Add((span.Start, span.End));
                }
            }

            // Merge overlaps for O(1) membership checks
            protectedSpans.Sort((a, b) => a.start.CompareTo(b.start));
            var merged = new List<(int start, int end)>();
            foreach (var s in protectedSpans)
            {
                if (merged.Count == 0 || s.start > merged[^1].end + 1)
                    merged.Add(s);
                else
                    merged[^1] = (merged[^1].start, Math.Max(merged[^1].end, s.end));
            }

            bool IsProtected(int idx)
            {
                // binary search over merged
                int lo = 0, hi = merged.Count - 1;
                while (lo <= hi)
                {
                    int mid = (lo + hi) >> 1;
                    var (s, e) = merged[mid];
                    if (idx < s) hi = mid - 1;
                    else if (idx > e) lo = mid + 1;
                    else return true;
                }
                return false;
            }

            // 2) Walk original markdown and insert extra "\" before (, ), [, ] when appropriate
            var src = markdown.AsSpan();
            var result = new System.Text.StringBuilder(markdown.Length + 16);

            for (int i = 0; i < src.Length; i++)
            {
                char c = src[i];

                if (!IsProtected(i) && c == '\\' && i + 1 < src.Length)
                {
                    char next = src[i + 1];

                    // We only care about MathJax delimiters \( \) \[ \]
                    if (next is '(' or ')' or '[' or ']')
                    {
                        // If it's already double-escaped (preceded by a backslash), leave it.
                        bool alreadyDouble = (i > 0 && src[i - 1] == '\\');

                        if (!alreadyDouble)
                        {
                            result.Append('\\'); // insert an extra backslash
                        }
                    }
                }

                result.Append(c);
            }

            return result.ToString();
        }
    }

}
