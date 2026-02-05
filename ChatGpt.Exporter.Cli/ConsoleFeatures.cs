namespace ChatGpt.Exporter.Cli
{
    internal static class ConsoleFeatures
    {
        public static void StartIndeterminate()
        {
            if (enableOscTabProgress)
            {
                Console.Write("\x1b]9;4;3\x07"); // https://learn.microsoft.com/en-us/windows/terminal/tutorials/progress-bar-sequences
            }
        }

        public static void SetProgress(int percent)
        {
            if (enableOscTabProgress)
            {
                Console.Write($"\x1b]9;4;1;{percent}\x07");
            }
        }

        public static void ClearState()
        {
            if (enableOscTabProgress)
            {
                Console.Write("\x1b]9;4;0;\x07");
            }
        }

        private static bool enableOscTabProgress = IsInteractiveTty() && SupportsOscTabProgress();

        private static bool IsInteractiveTty() =>
            !Console.IsOutputRedirected; // avoid dumping control bytes into logs/pipes

        private static bool SupportsOscTabProgress()
        {
            // Known-bad first
            var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM") ?? "";
            if (termProgram.Contains("iTerm", StringComparison.OrdinalIgnoreCase)) return false; // iTerm2
            if (Environment.GetEnvironmentVariable("KITTY_WINDOW_ID") is not null) return false; // kitty
            if (termProgram.Contains("Apple_Terminal", StringComparison.OrdinalIgnoreCase)) return false; // macOS Terminal
            if (termProgram.Equals("vscode", StringComparison.OrdinalIgnoreCase)) return false; // VS Code integrated

            // tmux/screen often block OSC; be conservative
            if (Environment.GetEnvironmentVariable("TMUX") is not null ||
                (Environment.GetEnvironmentVariable("TERM") ?? "").StartsWith("screen")) return false;

            // Known-good
            if (Environment.GetEnvironmentVariable("WT_SESSION") is not null) return true; // Windows Terminal
            if (Environment.GetEnvironmentVariable("VTE_VERSION") is not null) return true; // GNOME Terminal/VTE
            if (Environment.GetEnvironmentVariable("WEZTERM_EXECUTABLE") is not null ||
                termProgram.Contains("WezTerm", StringComparison.OrdinalIgnoreCase)) return true; // WezTerm
            if (termProgram.Contains("Ghostty", StringComparison.OrdinalIgnoreCase)) return true; // Ghostty

            return false; // default safe
        }
    }
}
