using System;
using System.Collections.Generic;
using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace ChatGPTExport.Exporters
{
    public static class MarkdownMathConverter
    {
        /// <summary>
        /// Converts backslash-delimited MathJax markers to dollar notation:
        ///   \( -> $ ,  \) -> $
        ///   \[ -> $$,  \] -> $$
        /// Skips text inside inline code spans and fenced/indented code blocks.
        /// No special handling for double backslashes is performed (per requirement).
        /// </summary>
        public static string ConvertBackslashMathToDollar(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return markdown ?? string.Empty;

            // Build a pipeline that fills SourceSpan so we can map code regions to original indices.
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UsePreciseSourceLocation()
                .Build();

            var doc = Markdown.Parse(markdown, pipeline);

            // Collect protected spans for any code nodes.
            var protectedSpans = new List<(int start, int end)>();
            foreach (var n in doc.Descendants())
            {
                if (n is CodeInline || n is FencedCodeBlock || n is CodeBlock)
                {
                    var s = n.Span; // indices into the ORIGINAL markdown
                    if (s.Start >= 0 && s.End >= s.Start)
                        protectedSpans.Add((s.Start, s.End));
                }
            }

            // Merge overlapping/adjacent spans to speed up lookups.
            protectedSpans.Sort((a, b) => a.start.CompareTo(b.start));
            var merged = new List<(int start, int end)>();
            foreach (var r in protectedSpans)
            {
                if (merged.Count == 0 || r.start > merged[^1].end + 1)
                    merged.Add(r);
                else
                    merged[^1] = (merged[^1].start, Math.Max(merged[^1].end, r.end));
            }

            static bool IsProtected(int idx, List<(int start, int end)> spans)
            {
                int lo = 0, hi = spans.Count - 1;
                while (lo <= hi)
                {
                    int mid = lo + hi >> 1;
                    var (s, e) = spans[mid];
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
                if (!IsProtected(i, merged) && src[i] == '\\' && i + 1 < src.Length)
                {
                    char n1 = src[i + 1];
                    if (n1 == '(')
                    {
                        sb.Append('$');
                        i += 2; // skip the '\('
                        continue;
                    }
                    if (n1 == ')')
                    {
                        sb.Append('$');
                        i += 2; // skip the '\)'
                        continue;
                    }
                    if (n1 == '[')
                    {
                        sb.Append("$$");
                        i += 2; // skip the '\['
                        continue;
                    }
                    if (n1 == ']')
                    {
                        sb.Append("$$");
                        i += 2; // skip the '\]'
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