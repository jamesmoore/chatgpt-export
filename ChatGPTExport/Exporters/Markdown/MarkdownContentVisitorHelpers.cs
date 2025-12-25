namespace ChatGPTExport.Exporters.Markdown
{
    internal static class MarkdownContentVisitorHelpers
    {

        /// <summary>
        /// Given some some prospective Markdown text, detect and replace any HTML tags that would break out of the Markdown renderer.
        /// Allow HTML tags within code blocks and pre-formatted text. 
        /// </summary>
        /// <param name="text">Potentially unsafe markdown</param>
        /// <returns>Markdown with </returns>
        public static string SanitizeMarkdown(string text)
        {
            var output = new List<string>();

            // The \n line ending detection is to preserve the same line ending (\r\n or just \n) as the original when the lines are recombined.
            // There is no guarantee that the input's line endings are consistent.
            // No provision is made for standalone \r line endings - these will just be treated as one big line.
            var lineSeparator = "\n";

            var lines = text.Split(lineSeparator);

            var linesWithFencedStatus = DetermineFenced(lines).ToList();

            for (int i = 0; i < linesWithFencedStatus.Count; i++)
            {
                var line = linesWithFencedStatus[i];
                var fenced = line.fenced;
                var lineText = line.fenced ? line.s : EscapeContents(line.s);
                bool dontReformatLineEndings =
                    fenced ||
                    i == linesWithFencedStatus.Count - 1 || // EOL
                    linesWithFencedStatus[i + 1].fenced || // Next line fenced
                    string.IsNullOrWhiteSpace(linesWithFencedStatus[i + 1].s) || // Next line empty 
                    lineText.EndsWith("  "); // Alread has break indicator
                output.Add(dontReformatLineEndings ? lineText : ReformatLineEndings(lineText));
            }

            return string.Join(lineSeparator, output);
        }


        private static IEnumerable<(string s, bool fenced)> DetermineFenced(IEnumerable<string> lines)
        {
            var fenced = false;
            var fencedTerminator = null as string;
            foreach (var line in lines)
            {
                if (line.StartsWith("    ", StringComparison.Ordinal)) // don't sanitize preformatted or tagless
                {
                    yield return (line, true);
                    continue;
                }

                var trimmedLine = line.TrimStart();
                if (fenced == false)
                {
                    if (trimmedLine.StartsWith("```", StringComparison.Ordinal) || trimmedLine.StartsWith("~~~", StringComparison.Ordinal))
                    {
                        var blockchar = trimmedLine[0];
                        var count = trimmedLine.TakeWhile(p => p == blockchar).Count();
                        fencedTerminator = new string(blockchar, count);
                        fenced = true;
                        yield return (line, true);
                        continue;
                    }
                }
                else
                {
                    if (trimmedLine.StartsWith(fencedTerminator))
                    {
                        fenced = false;
                        yield return (line, true);
                        continue;
                    }
                }

                yield return (line, fenced);
            }
        }

        static readonly List<string> relevantTags =
            [
                "<style",
                "<script",
                "<pre",
                "<!--",
                // maybe add others
            ];

        private static string ReformatLineEndings(string line)
        {
            if (string.IsNullOrWhiteSpace(line.Trim()))
            {
                return line;
            }

            return line + "\n";
        }

        private static string EscapeContents(string line)
        {
            if (line.Contains('<') == false || line.Contains('>') == false)
            {
                return line;
            }

            if (relevantTags.Any(p => line.Contains(p, StringComparison.OrdinalIgnoreCase)) == false)
            {
                return line;
            }

            var prefix = line.StartsWith("> ", StringComparison.Ordinal) ? "> " : string.Empty;
            line = line.Substring(prefix.Length);

            var parts = line.Split('`');
            for (int i = 0; i < parts.Length; i += 2) // even indices: outside code
            {
                parts[i] = parts[i]
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");
            }

            return prefix + string.Join("`", parts);
        }
    }
}