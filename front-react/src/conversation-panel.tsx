import { useParams } from "react-router-dom";
import { useState, useEffect } from "react";
import { useConversation } from "./hooks/use-conversation";
import { getWrapStatus } from "./getWrapStatus";

export function ConversationPanel() {

    const { id, format } = useParams();
    const { data: content, error } = useConversation(id, format);
    
    const [isWrapped, setIsWrapped] = useState(() => getWrapStatus());

    useEffect(() => {
        const handleStorageChange = () => {
            setIsWrapped(getWrapStatus());
        };
        window.addEventListener('storage', handleStorageChange);
        return () => window.removeEventListener('storage', handleStorageChange);
    }, []);

    if (!id) {
        return <>No conversation ID provided.</>;
    }

    if (error) {
        return <>{error instanceof Error ? error.message : "Failed to load conversation."}</>;
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
        return (
            <pre className={`p-4 max-w-full overflow-x-auto ${isWrapped ? 'whitespace-pre-wrap wrap-break-word' : ''}`}>
                <code>{content}</code>
            </pre>
        );
    }

    return (<>{content || ""}</>);
}
