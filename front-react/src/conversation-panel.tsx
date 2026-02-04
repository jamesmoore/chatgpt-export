import { useParams } from "react-router-dom";
import { useRef, useEffect } from "react";
import { useConversation } from "./hooks/use-conversation";

export function ConversatrionPanel() {

    const { id, format } = useParams();
    const { data: content, error } = useConversation(id, format);
    const iframeRef = useRef<HTMLIFrameElement>(null);

    useEffect(() => {
        const iframe = iframeRef.current;
        if (!iframe) return;

        const resizeIframe = () => {
            try {
                const iframeDocument = iframe.contentDocument || iframe.contentWindow?.document;
                if (iframeDocument) {
                    const height = iframeDocument.documentElement.scrollHeight;
                    // Add extra space to account for horizontal scrollbar if present
                    iframe.style.height = `${height + 20}px`;
                }
            } catch (e) {
                console.error('Failed to resize iframe:', e);
            }
        };

        iframe.addEventListener('load', resizeIframe);
        return () => iframe.removeEventListener('load', resizeIframe);
    }, [content]);

    if (!id) {
        return <>No conversation ID provided.</>;
    }

    if (error) {
        return <>{error instanceof Error ? error.message : "Failed to load conversation."}</>;
    }

    if (format === 'html' && content) {
        return (
            <iframe
                ref={iframeRef}
                srcDoc={content}
                style={{ width: '100%', border: 'none', display: 'block' }}
                title="Conversation HTML"
            />
        );
    }

    if (format === 'markdown' || format === 'json') {
        return (
            <pre className="p-4">
                <code>{content}</code>
            </pre>
        );
    }

    return (<>{content || ""}</>);
}
