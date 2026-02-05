import { useParams } from "react-router-dom";
import { useState, useEffect } from "react";
import hljs from "highlight.js";
import githubStyles from "highlight.js/styles/github.css?inline";
import githubDarkStyles from "highlight.js/styles/github-dark.css?inline";
import { useConversation } from "./hooks/use-conversation";
import { getWrapStatus } from "./getWrapStatus";
import { useTheme } from "./components/theme-provider";
import LoadingSpinner from "./loading-spinner";

export function ConversationPanel() {

    const { id, format } = useParams();
    const { data: content, error, isLoading } = useConversation(id, format);
    const { theme } = useTheme();

    const [isWrapped, setIsWrapped] = useState(() => getWrapStatus());

    useEffect(() => {
        const handleStorageChange = () => {
            setIsWrapped(getWrapStatus());
        };
        window.addEventListener('storage', handleStorageChange);
        return () => window.removeEventListener('storage', handleStorageChange);
    }, []);

    useEffect(() => {
        const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
        const resolvedTheme = theme === "system" ? (mediaQuery.matches ? "dark" : "light") : theme;
        const styleId = "conversation-panel-hljs-theme";
        let style = document.getElementById(styleId) as HTMLStyleElement | null;

        if (!style) {
            style = document.createElement("style");
            style.id = styleId;
            document.head.appendChild(style);
        }

        style.textContent = resolvedTheme === "dark" ? githubDarkStyles : githubStyles;

        if (theme !== "system") return;

        const handleChange = (event: MediaQueryListEvent) => {
            if (!style) return;
            style.textContent = event.matches ? githubDarkStyles : githubStyles;
        };

        mediaQuery.addEventListener("change", handleChange);
        return () => mediaQuery.removeEventListener("change", handleChange);
    }, [theme]);

    if (!id) {
        return <>No conversation ID provided.</>;
    }

    if (error) {
        return <>{error instanceof Error ? error.message : "Failed to load conversation."}</>;
    }

    if (isLoading) {
        return <LoadingSpinner />;
    }

    if (format === "html" && content) {
        return (
            <iframe
                srcDoc={content}
                className="flex-1 w-full border-none"
                title="Conversation HTML"
            />
        );
    }

    if (format === 'markdown' || format === 'json') {
        const value = content || "";
        const highlighted = format === "json"
            ? hljs.highlight(value, { language: "json", ignoreIllegals: true }).value
            : hljs.highlight(value, { language: "markdown", ignoreIllegals: true }).value;

        return (
            <div className='overflow-x-auto'>
                <pre className={isWrapped ? 'whitespace-pre-wrap break-words' : 'min-w-max whitespace-pre'}>
                    <code
                        className={`bg-transparent! hljs language-${format}`}
                        dangerouslySetInnerHTML={{ __html: highlighted }}
                    />
                </pre>
            </div>
        );
    }

    return (<>{content || ""}</>);
}
