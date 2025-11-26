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
            var fenced = false;
            var fencedTerminator = null as string;
            var output = new List<string>();

            // The \n line ending detection is to preserve the same line ending (\r\n or just \n) as the original when the lines are recombined.
            // There is no guarantee that the input's line endings are consistent.
            // No provision is made for standalone \r line endings - these will just be treated as one big line.
            var lineSeparator = "\n";

            foreach (var line in text.Split(lineSeparator))
            {
                if (line.StartsWith("    ", StringComparison.Ordinal)) // don't sanitize preformatted or tagless
                {
                    output.Add(line);
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
                        output.Add(line);
                        continue;
                    }
                }
                else
                {
                    if (trimmedLine.StartsWith(fencedTerminator))
                    {
                        fenced = false;
                        output.Add(line);
                        continue;
                    }
                }

                output.Add(fenced ? line : Escape(line));
            }

            return string.Join(lineSeparator, output);
        }

        static readonly List<string> relevantTags =
            [
                "<style",
                "<script",
                "<pre",
                "<!--",
                // maybe add others
            ];

        private static string Escape(string line)
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